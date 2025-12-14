using Microsoft.EntityFrameworkCore;
using PlantHub.Web.Domain;
using PlantHub.Web.Infrastructure;

namespace PlantHub.Web.Lib;

public sealed class WateringMonitorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WateringMonitorService> _logger;

    // In-memory guard: max en körning per dag
    private DateOnly? _lastRunDate;

    // Kör varje dag kl 08:00–08:15
    private static readonly TimeSpan RunAt = new(8, 0, 0);
    private static readonly TimeSpan RunWindow = TimeSpan.FromMinutes(15);

    public WateringMonitorService(
        IServiceScopeFactory scopeFactory,
        ILogger<WateringMonitorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[WateringMonitor] Started");

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                var now = DateTime.Now;

                if (!ShouldRunNow(now))
                    continue;

                await CheckPlantsAsync(stoppingToken);

                _lastRunDate = DateOnly.FromDateTime(now);
            }
        }
        catch (OperationCanceledException)
        {
            // normal shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WateringMonitor] Fatal error");
        }
        finally
        {
            _logger.LogInformation("[WateringMonitor] Stopped");
        }
    }

    private bool ShouldRunNow(DateTime now)
    {
        var today = DateOnly.FromDateTime(now);

        if (_lastRunDate == today)
            return false;

        var t = now.TimeOfDay;
        return t >= RunAt && t < RunAt + RunWindow;
    }

    private async Task CheckPlantsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var checker = scope.ServiceProvider.GetRequiredService<WateringCheckService>();
        await checker.RunAsync(ct);
    }
}
