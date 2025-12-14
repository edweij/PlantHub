using Microsoft.Extensions.Options;

namespace PlantHub.Web.Lib;

public sealed class PlantHubNotificationService
{
    private readonly IHomeAssistantClient _ha;
    private readonly NotificationSettings _settings;

    public PlantHubNotificationService(
        IHomeAssistantClient ha,
        IOptions<NotificationSettings> options)
    {
        _ha = ha;
        _settings = options.Value;
    }

    public async Task NotifyAsync(
        string title,
        string message,
        CancellationToken ct = default)
    {
        // Always create persistent notification
        await _ha.CreatePersistentNotificationAsync(title, message, ct);

        // Optional push
        if (!_settings.AllowPush || !_settings.PushEnabled)
            return;

        foreach (var service in _settings.PushNotifyServices)
        {
            if (string.IsNullOrWhiteSpace(service))
                continue;

            await _ha.SendPushNotificationAsync(
                service,
                title,
                message,
                ct);
        }
    }
}
