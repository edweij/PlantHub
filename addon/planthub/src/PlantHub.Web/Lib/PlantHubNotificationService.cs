using Microsoft.Extensions.Options;

namespace PlantHub.Web.Lib;

public sealed class PlantHubNotificationService
{
    private readonly IHomeAssistantClient _ha;
    private readonly NotificationSettings _settings;
    private readonly ILogger<PlantHubNotificationService> _logger;

    public PlantHubNotificationService(
        IHomeAssistantClient ha,
        ILogger<PlantHubNotificationService> logger,
        IOptions<NotificationSettings> options)
    {
        _ha = ha;
        _logger = logger;
        _settings = options.Value;
    }

    public async Task NotifyAsync(
        string title,
        string message,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[Notifications] Sending persistent notification. Push allowed={AllowPush}, push enabled={PushEnabled}, push services={Services}",
            _settings.AllowPush,
            _settings.PushEnabled,
            _settings.PushNotifyServices.Length == 0 ? "<none>" : string.Join(", ", _settings.PushNotifyServices));

        // Always create persistent notification
        await _ha.CreatePersistentNotificationAsync(title, message, ct);

        // Optional push
        if (!_settings.AllowPush || !_settings.PushEnabled)
        {
            _logger.LogInformation("[Notifications] Push skipped because AllowPush={AllowPush} and PushEnabled={PushEnabled}",
                _settings.AllowPush,
                _settings.PushEnabled);
            return;
        }

        foreach (var service in _settings.PushNotifyServices)
        {
            if (string.IsNullOrWhiteSpace(service))
            {
                _logger.LogWarning("[Notifications] Encountered empty push notify service entry");
                continue;
            }

            try
            {
                _logger.LogInformation("[Notifications] Sending push via {Service}", service);
                await _ha.SendPushNotificationAsync(
                    service,
                    title,
                    message,
                    ct);
                _logger.LogInformation("[Notifications] Push sent via {Service}", service);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Notifications] Failed to send push via {Service}", service);
            }
        }
    }
}
