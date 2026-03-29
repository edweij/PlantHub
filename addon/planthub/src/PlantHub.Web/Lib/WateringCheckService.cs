using Microsoft.EntityFrameworkCore;
using PlantHub.Web.Domain;
using PlantHub.Web.Infrastructure;

namespace PlantHub.Web.Lib;

public sealed class WateringCheckService
{
    private readonly PlantHubDbContext _db;
    private readonly IHomeAssistantClient _haClient;
    private readonly PlantHubNotificationService _notifications;
    private readonly ILogger<WateringCheckService> _logger;

    public WateringCheckService(
        PlantHubDbContext db,
        IHomeAssistantClient haClient,
        PlantHubNotificationService notifications,
        ILogger<WateringCheckService> logger)
    {
        _db = db;
        _haClient = haClient;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("[WateringCheck] Running manual/daily check");

        var plants = await _db.Plants
            .Where(p => p.IsActive && p.Mode == WateringMode.Schedule)
            .Include(p => p.WateringGroup)
            .Include(p => p.WateringEvents)
            .ToListAsync(ct);

        foreach (var plant in plants)
        {
            var daysOverdue = plant.GetDaysOverdue();
            if (daysOverdue is null || daysOverdue <= 0)
                continue;

            var nextWaterDate = plant.ComputeNextWaterDate();
            if (nextWaterDate is null)
                continue;

            if (plant.LastWateringReminderUtc?.Date >= nextWaterDate.Value.Date)
            {
                _logger.LogDebug(
                    "[WateringCheck] Skipping {Plant}; reminder already sent for cycle due {DueDate:d}",
                    plant.Name,
                    nextWaterDate.Value);
                continue;
            }

            await _notifications.NotifyAsync(
                "🌿 Plant needs watering",
                $"{plant.Name} needs watering today",
                ct);

            plant.LastWateringReminderUtc = DateTime.UtcNow;

            _logger.LogInformation(
                "[WateringCheck] Notified overdue plant: {Plant}",
                plant.Name);
        }

        var sensorPlants = await _db.Plants
            .Where(p => p.IsActive &&
                        p.Mode == WateringMode.Sensor &&
                        p.SensorEntityId != null &&
                        p.MoistureLowPercent != null)
            .ToListAsync(ct);

        foreach (var plant in sensorPlants)
        {
            var state = await _haClient.GetEntityStateAsync(plant.SensorEntityId!, ct);
            if (state is null)
            {
                _logger.LogWarning(
                    "[WateringCheck] Could not load state for sensor plant {Plant} ({EntityId})",
                    plant.Name,
                    plant.SensorEntityId);
                continue;
            }

            if (!double.TryParse(
                state.State,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var moisture))
            {
                _logger.LogWarning(
                    "[WateringCheck] Sensor state for {Plant} was not numeric: {State}",
                    plant.Name,
                    state.State);
                continue;
            }

            var lowThreshold = plant.MoistureLowPercent!.Value;
            var resetThreshold = plant.MoistureHighPercent ?? Math.Min(lowThreshold + 5, 100);

            if (moisture > resetThreshold)
            {
                if (plant.LastWateringReminderUtc is not null)
                {
                    plant.LastWateringReminderUtc = null;
                    _logger.LogInformation(
                        "[WateringCheck] Reset dry reminder for {Plant}; moisture recovered to {Moisture:0.#}%",
                        plant.Name,
                        moisture);
                }

                continue;
            }

            if (moisture > lowThreshold)
                continue;

            if (plant.LastWateringReminderUtc is not null)
            {
                _logger.LogDebug(
                    "[WateringCheck] Skipping sensor alert for {Plant}; already notified in current dry cycle",
                    plant.Name);
                continue;
            }

            await _notifications.NotifyAsync(
                "🌿 Plant needs watering",
                $"{plant.Name} is dry ({moisture:0.#}%). Threshold: {lowThreshold:0.#}%.",
                ct);

            plant.LastWateringReminderUtc = DateTime.UtcNow;

            _logger.LogInformation(
                "[WateringCheck] Notified dry sensor plant: {Plant} ({Moisture:0.#}%)",
                plant.Name,
                moisture);
        }

        await _db.SaveChangesAsync(ct);
    }
}
