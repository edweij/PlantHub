using Microsoft.EntityFrameworkCore;
using PlantHub.Web.Domain;
using PlantHub.Web.Infrastructure;

namespace PlantHub.Web.Lib;

public sealed class WateringCheckService
{
    private readonly PlantHubDbContext _db;
    private readonly PlantHubNotificationService _notifications;
    private readonly ILogger<WateringCheckService> _logger;

    public WateringCheckService(
        PlantHubDbContext db,
        PlantHubNotificationService notifications,
        ILogger<WateringCheckService> logger)
    {
        _db = db;
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

        await _db.SaveChangesAsync(ct);
    }
}
