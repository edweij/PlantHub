// HomeAssistant/HomeAssistantLlTokenClient.cs
using System.Net.WebSockets;
using System.Text.Json;

namespace PlantHub.Web.Lib;

public sealed class HomeAssistantLlTokenClient : AbstractHomeAssistantClient
{
    public HomeAssistantLlTokenClient(string? baseUrl, string? token) : base(baseUrl, token) { }

    public override async Task<IReadOnlyList<HaAreaLite>> GetAreasAsync(CancellationToken ct = default)
    {
        if (!IsEnabled) return Array.Empty<HaAreaLite>();

        var probeUri = BaseUri!;
        try
        {
            var code = await ProbeHttpAsync(probeUri, Token!, ct);
            Console.WriteLine($"[PlantHub] Probe {probeUri} -> HTTP {code}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PlantHub] Probe {probeUri} failed: {ex.Message}");
        }

        var wsUri = BuildWebSocketUri(BaseUri!);
        using var ws = new ClientWebSocket();
        ws.Options.SetRequestHeader("Authorization", $"Bearer {Token}");
        ws.Options.SetRequestHeader("Origin", BaseUri!.GetLeftPart(UriPartial.Authority));
        ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        await ws.ConnectAsync(wsUri, cts.Token);

        // auth_required
        _ = await ReceiveJsonAsync(ws, ct);
        // auth
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

    public override async Task CreatePersistentNotificationAsync(
    string title,
    string message,
    CancellationToken ct = default)
    {
        if (!IsEnabled) return;

        var payload = new
        {
            title,
            message
        };

        await PostJsonAsync(
            "api/services/persistent_notification/create",
            payload,
            ct);
    }

    public override async Task SendPushNotificationAsync(
    string notifyService,
    string title,
    string message,
    CancellationToken ct = default)
    {
        if (!IsEnabled) return;

        var parts = notifyService.Split('.', 2);
        if (parts.Length != 2) return;

        var payload = new
        {
            title,
            message
        };

        await PostJsonAsync(
            $"api/services/{parts[0]}/{parts[1]}",
            payload,
            ct);
    }
}
