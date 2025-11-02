using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace PlantHub.Web.Lib;

public record HaAreaLite(string Id, string Name, string? FloorId);

public interface IHomeAssistantClient
{
    bool IsEnabled { get; }
    Task<IReadOnlyList<HaAreaLite>> GetAreasAsync(CancellationToken ct = default);
}

public sealed class HomeAssistantClient : IHomeAssistantClient
{
    private readonly Uri? _baseUri;
    private readonly string? _token;

    public bool IsEnabled => _baseUri is not null && !string.IsNullOrWhiteSpace(_token);

    public HomeAssistantClient(string? baseUrl, string? token)
    {
        if (!string.IsNullOrWhiteSpace(baseUrl) && !string.IsNullOrWhiteSpace(token))
        {
            _baseUri = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/");
            _token = token;
        }
    }

    public async Task<IReadOnlyList<HaAreaLite>> GetAreasAsync(CancellationToken ct = default)
    {
        if (!IsEnabled) return Array.Empty<HaAreaLite>();

        var wsUri = BuildWebSocketUri(_baseUri!);
        using var ws = new ClientWebSocket();

        if (!string.IsNullOrWhiteSpace(_token))
            ws.Options.SetRequestHeader("Authorization", $"Bearer {_token}");

        ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);                                                                  
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        await ws.ConnectAsync(wsUri, cts.Token);

        await ws.ConnectAsync(wsUri, ct);

        // auth_required
        _ = await ReceiveJsonAsync(ws, ct);
        // auth
        await SendJsonAsync(ws, new { type = "auth", access_token = _token }, ct);
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
            var list = new List<HaAreaLite>();
            foreach (var el in result.EnumerateArray())
            {
                list.Add(new HaAreaLite(el.GetProperty("area_id").GetString() ?? "",
                    el.GetProperty("name").GetString() ?? "",
                    el.TryGetProperty("floor_id", out var f) && f.ValueKind != JsonValueKind.Null
                                ? f.GetString()
                                : null));                
            }
            return list;
        }

        return Array.Empty<HaAreaLite>();
    }

    // --- helpers ---
    private static Uri BuildWebSocketUri(Uri baseUri)
    {
        var scheme = baseUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? "wss" : "ws";
        var path = baseUri.AbsolutePath.TrimEnd('/');

        string wsPath;
        if (path.EndsWith("/core/api", StringComparison.OrdinalIgnoreCase))
            wsPath = path[..^4] + "websocket";   // "/core/api" -> "/core/websocket"
        else if (path.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
            wsPath = path[..^3] + "websocket";   // "/api" -> "/websocket"
        else
            wsPath = (path.Length == 0 ? "" : path) + "/api/websocket";

        var ub = new UriBuilder(baseUri) { Scheme = scheme, Path = wsPath };
        return ub.Uri;
    }

    private static async Task SendJsonAsync(ClientWebSocket ws, object obj, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(obj);
        var bytes = Encoding.UTF8.GetBytes(json);
        await ws.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
    }

    private static async Task<JsonElement> ReceiveJsonAsync(ClientWebSocket ws, CancellationToken ct)
    {
        var buffer = new byte[32 * 1024];
        using var ms = new MemoryStream();
        WebSocketReceiveResult result;
        do
        {
            result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
            if (result.MessageType == WebSocketMessageType.Close)
                throw new WebSocketException("WebSocket closed by server");
            ms.Write(buffer, 0, result.Count);
        } while (!result.EndOfMessage);

        using var doc = JsonDocument.Parse(ms.ToArray());
        return doc.RootElement.Clone();
    }
}
