namespace PlantHub.Web.Lib;

public sealed class DisabledHomeAssistantClient : IHomeAssistantClient
{
    public bool IsEnabled => false;

    public Task<IReadOnlyList<HaAreaLite>> GetAreasAsync(CancellationToken ct = default)
        => Task.FromResult((IReadOnlyList<HaAreaLite>)Array.Empty<HaAreaLite>());

    public Task<IReadOnlyList<HaEntityStateLite>> GetSoilMoistureSensorsAsync(CancellationToken ct = default)
        => Task.FromResult((IReadOnlyList<HaEntityStateLite>)Array.Empty<HaEntityStateLite>());

    public Task<HaEntityStateLite?> GetEntityStateAsync(string entityId, CancellationToken ct = default)
        => Task.FromResult<HaEntityStateLite?>(null);

    public Task CreatePersistentNotificationAsync(
        string title,
        string message,
        CancellationToken ct = default)
    {
        // Intentionally no-op
        return Task.CompletedTask;
    }

    public Task SendPushNotificationAsync(
    string notifyService,
    string title,
    string message,
    CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}
