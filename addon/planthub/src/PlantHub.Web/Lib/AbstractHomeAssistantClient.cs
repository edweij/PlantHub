// HomeAssistant/AbstractHomeAssistantClient.cs
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace PlantHub.Web.Lib;

// IHomeAssistantClient.cs (du hade redan detta)
public interface IHomeAssistantClient
{
    bool IsEnabled { get; }
    Task<IReadOnlyList<HaAreaLite>> GetAreasAsync(CancellationToken ct = default);

    Task CreatePersistentNotificationAsync(
        string title,
        string message,
        CancellationToken ct = default);
    Task SendPushNotificationAsync(
        string notifyService,
        string title,
        string message,
        CancellationToken ct = default);
}

public abstract class AbstractHomeAssistantClient : IHomeAssistantClient
{
    protected readonly Uri? BaseUri;
    protected readonly string? Token;

    public bool IsEnabled => BaseUri is not null && !string.IsNullOrWhiteSpace(Token);

    protected AbstractHomeAssistantClient(string? baseUrl, string? token)
    {
        if (!string.IsNullOrWhiteSpace(baseUrl) && !string.IsNullOrWhiteSpace(token))
        {
            BaseUri = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/");
            Token = token;
        }
    }

    public abstract Task<IReadOnlyList<HaAreaLite>> GetAreasAsync(CancellationToken ct = default);

    public abstract Task CreatePersistentNotificationAsync(string title, string message, CancellationToken ct = default);

    public abstract Task SendPushNotificationAsync(
        string notifyService,
        string title,
        string message,
        CancellationToken ct = default);

    protected static Uri BuildWebSocketUri(Uri baseUri)
    {
        var scheme = baseUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? "wss" : "ws";
        var path = baseUri.AbsolutePath.TrimEnd('/'); // ex: "/core/api"

        string wsPath;
        if (path.EndsWith("/core/api", StringComparison.OrdinalIgnoreCase))
        {
            // "/core/api" -> "/core/websocket"
            wsPath = path[..^4] + "/websocket"; // lägg till "/" före websocket
        }
        else if (path.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            // "/api" -> "/websocket"
            wsPath = path[..^3] + "websocket";   // här finns redan ett "/" före "api"
        }
        else
        {
            // fallback: lägg till "/api/websocket"
            wsPath = (path.Length == 0 ? "" : path) + "/api/websocket";
        }

        var ub = new UriBuilder(baseUri) { Scheme = scheme, Path = wsPath };
        return ub.Uri;
    }

    protected static async Task SendJsonAsync(ClientWebSocket ws, object obj, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(obj);
        var bytes = Encoding.UTF8.GetBytes(json);
        await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, ct);
    }

    protected static async Task<JsonElement> ReceiveJsonAsync(ClientWebSocket ws, CancellationToken ct)
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

    protected async Task<int> ProbeHttpAsync(Uri baseUri, string token, CancellationToken ct)
    {
        using var http = new HttpClient { BaseAddress = baseUri };
        if (!string.IsNullOrWhiteSpace(token))
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        using var resp = await http.GetAsync("", ct);
        return (int)resp.StatusCode;
    }

    protected async Task PostJsonAsync(string relativePath, object payload, CancellationToken ct)
    {
        if (!IsEnabled || BaseUri is null || Token is null)
            return;

        using var http = new HttpClient { BaseAddress = BaseUri };
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

        using var resp = await http.PostAsJsonAsync(relativePath, payload, ct);
        resp.EnsureSuccessStatusCode();
    }

    protected static List<HaAreaLite> ParseAreaList(JsonElement result)
    {
        var list = new List<HaAreaLite>();
        foreach (var el in result.EnumerateArray())
        {
            var id = el.GetProperty("area_id").GetString() ?? "";
            var name = el.GetProperty("name").GetString() ?? "";
            string? floor = null;
            if (el.TryGetProperty("floor_id", out var f) && f.ValueKind != JsonValueKind.Null)
                floor = f.GetString();
            list.Add(new HaAreaLite(id, name, floor));
        }
        return list;
    }
}
