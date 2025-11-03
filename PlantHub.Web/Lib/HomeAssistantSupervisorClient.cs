using System.Net.WebSockets;
using System.Text.Json;

namespace PlantHub.Web.Lib;

public sealed class HomeAssistantSupervisorClient : AbstractHomeAssistantClient
{
    // note: baseUrl should typically be "http://supervisor/core/api" when running as addon
    public HomeAssistantSupervisorClient(string? baseUrl, string? token) : base(baseUrl, token) { }

    public override async Task<IReadOnlyList<HaAreaLite>> GetAreasAsync(CancellationToken ct = default)
    {
        if (!IsEnabled) return Array.Empty<HaAreaLite>();

        var probeUri = BaseUri!;
        try
        {
            // For supervisor proxy we might not be allowed Authorization header, but a simple probe is fine
            var code = await ProbeHttpAsync(probeUri, Token!, ct);
            Console.WriteLine($"[PlantHub] (supervisor) Probe {probeUri} -> HTTP {code}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PlantHub] (supervisor) Probe {probeUri} failed: {ex.Message}");
        }

        var wsUri = BuildWebSocketUri(BaseUri!);
        using var ws = new ClientWebSocket();

        // Supervisor expects X-Supervisor-Token header. Authorization is usually not required.
        ws.Options.SetRequestHeader("X-Supervisor-Token", Token);
        // Set Origin to HA ingress authority so the proxy will accept it
        ws.Options.SetRequestHeader("Origin", BaseUri!.GetLeftPart(UriPartial.Authority));
        ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        await ws.ConnectAsync(wsUri, cts.Token);

        // auth_required
        _ = await ReceiveJsonAsync(ws, ct);
        // For supervisor proxy, Home Assistant still expects "auth" with access_token
        await SendJsonAsync(ws, new { type = "auth", access_token = Token }, ct);
        // auth_ok
        _ = await ReceiveJsonAsync(ws, ct);

        // request areas
        await SendJsonAsync(ws, new { id = 1, type = "config/area_registry/list" }, ct);

        var resp = await ReceiveJsonAsync(ws, ct);
        if (resp.TryGetProperty("type", out var t) && t.GetString() == "result" &&
            resp.TryGetProperty("success", out var s) && s.GetBoolean() &&
            resp.TryGetProperty("result", out var result) &&
            result.ValueKind == JsonValueKind.Array)
        {
            return ParseAreaList(result);
        }

        return Array.Empty<HaAreaLite>();
    }
}
