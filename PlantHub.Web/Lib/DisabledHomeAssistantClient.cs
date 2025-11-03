namespace PlantHub.Web.Lib;

public sealed class DisabledHomeAssistantClient : IHomeAssistantClient
{
    public bool IsEnabled => false;
    public Task<IReadOnlyList<HaAreaLite>> GetAreasAsync(CancellationToken ct = default) => Task.FromResult((IReadOnlyList<HaAreaLite>)Array.Empty<HaAreaLite>());
}
