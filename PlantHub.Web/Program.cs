using PlantHub.Web.Components;
using PlantHub.Web.Lib;



var builder = WebApplication.CreateBuilder(args);

// ---- Read env/config ----
var cfg = builder.Configuration;
var supervisorToken = Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN");

//var envBaseUrl = Environment.GetEnvironmentVariable("PLANTHUB__BASEURL"); // e.g. http://homeassistant:8123
//var envToken = Environment.GetEnvironmentVariable("PLANTHUB__TOKEN");

//var haBaseUrlRaw = cfg["HA:BaseUrl"] ?? envBaseUrl;   // might be without /api
//var haToken = cfg["HA:Token"] ?? envToken;

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddSingleton<IHomeAssistantClient>(_ =>
{
    // 1) Home Assistant add-on: Supervisor-token vinner alltid
    var supervisorToken = Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN");
    if (!string.IsNullOrWhiteSpace(supervisorToken))
        return new HomeAssistantClient("http://supervisor/core/api", supervisorToken);

    // 2) Lokal/debug: läs från config eller env (LL token)
    //    Tillåt både "HA:BaseUrl"/"HA:Token" (appsettings/user-secrets)
    //    och fallback till PLANTHUB__BASEURL/PLANTHUB__TOKEN (env)
    var cfg = builder.Configuration;
    var baseUrl = cfg["HA:BaseUrl"] ?? Environment.GetEnvironmentVariable("PLANTHUB__BASEURL");   // e.g. http://homeassistant.local:8123
    var token = cfg["HA:Token"] ?? Environment.GetEnvironmentVariable("PLANTHUB__TOKEN");

    if (!string.IsNullOrWhiteSpace(baseUrl) && !string.IsNullOrWhiteSpace(token))
        return new HomeAssistantClient(baseUrl, token);

    // 3) Annars avstängt (UI kan visa "disabled")
    return new HomeAssistantClient(null, null);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


app.UseAntiforgery();

app.MapStaticAssets();
app.MapGet("/health", () => Results.Ok(new { ok = true, time = DateTimeOffset.UtcNow }));

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
