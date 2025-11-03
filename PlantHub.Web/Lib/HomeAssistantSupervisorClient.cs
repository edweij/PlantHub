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
            var code = await ProbeHttpAsync(probeUri, Token!, ct);
            Console.WriteLine($"[PlantHub] (supervisor) Probe {probeUri} -> HTTP {code}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PlantHub] (supervisor) Probe {probeUri} failed: {ex.Message}");
        }

        var wsUri = BuildWebSocketUri(BaseUri!);
        Console.WriteLine($"[PlantHub] (supervisor) WS connect → {wsUri}");

        using var ws = new ClientWebSocket();
                
        ws.Options.SetRequestHeader("Authorization", $"Bearer {Token}");

        // Tips: testa UTAN Origin först. Lägg bara till vid behov.
        // ws.Options.SetRequestHeader("Origin", BaseUri!.GetLeftPart(UriPartial.Authority));

        ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(15));

        try
        {
            await ws.ConnectAsync(wsUri, cts.Token);
        }
        catch (WebSocketException wex)
        {
            Console.WriteLine($"[PlantHub] (supervisor) WS connect failed: {wex.Message}");
            throw; // låt Blazor få stacktrace men vi har nu mer context i loggen
        }

        // auth_required
        var first = await ReceiveJsonAsync(ws, ct);
        string firstType = first.TryGetProperty("type", out var ft) ? ft.GetString() ?? "<null>" : "<no type>";
        Console.WriteLine($"[PlantHub] (supervisor) WS first msg: {firstType}");

        // auth
        await SendJsonAsync(ws, new { type = "auth", access_token = Token }, ct);

        // auth_ok
        var authOk = await ReceiveJsonAsync(ws, ct);
        var authOkType = authOk.TryGetProperty("type", out var at) ? at.GetString() ?? "<null>" : "<no type>";
        Console.WriteLine($"[PlantHub] (supervisor) WS auth reply: {authOkType}");

        // request areas
        await SendJsonAsync(ws, new { id = 1, type = "config/area_registry/list" }, ct);

        var resp = await ReceiveJsonAsync(ws, ct);
        if (resp.TryGetProperty("type", out var t) && t.GetString() == "result" &&
            resp.TryGetProperty("success", out var s) && s.GetBoolean() &&
            resp.TryGetProperty("result", out var result) &&
            result.ValueKind == JsonValueKind.Array)
        {
            var list = ParseAreaList(result);
            Console.WriteLine($"[PlantHub] (supervisor) Areas = {list.Count}");
            return list;
        }

        Console.WriteLine($"[PlantHub] (supervisor) Unexpected WS response: {resp}");
        return Array.Empty<HaAreaLite>();
    }
}
