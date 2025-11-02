using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using PlantHub.Web.Components;
using PlantHub.Web.Lib;

var builder = WebApplication.CreateBuilder(args);

// ---- Read env/config ----
var cfg = builder.Configuration;
var supervisorToken = Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN");

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var dataProtectionKeys = "/data/aspnet-keys"; // add-onens persistenta volym
Directory.CreateDirectory(dataProtectionKeys);

// --- Ingress / Reverse proxy fix ---
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                               ForwardedHeaders.XForwardedProto |
                               ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});


builder.Services.AddHttpContextAccessor();
builder.Services.AddServerSideBlazor().AddCircuitOptions(options => options.DetailedErrors = true);

// --- HA Client injection ---
builder.Services.AddSingleton<IHomeAssistantClient>(_ =>
{
    // Läs båda varianterna
    var supervisorToken =
        Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN")
        ?? Environment.GetEnvironmentVariable("HASSIO_TOKEN");

    if (!string.IsNullOrWhiteSpace(supervisorToken))
        return new HomeAssistantClient("http://supervisor/core/api", supervisorToken);

    var cfg = builder.Configuration;
    var baseUrl = cfg["HA:BaseUrl"] ?? Environment.GetEnvironmentVariable("PLANTHUB__BASEURL");
    var token = cfg["HA:Token"] ?? Environment.GetEnvironmentVariable("PLANTHUB__TOKEN");

    if (!string.IsNullOrWhiteSpace(baseUrl) && !string.IsNullOrWhiteSpace(token))
        return new HomeAssistantClient(baseUrl, token);

    // valfritt: liten logg
    Console.WriteLine("[PlantHub] HomeAssistantClient disabled (no token).");
    return new HomeAssistantClient(null, null);
});

builder.WebHost.UseStaticWebAssets();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// --- Apply forwarded headers (HA proxy / ingress) ---
app.UseForwardedHeaders();

// --- Handle HA Ingress path / reverse proxy prefix ---
app.Use((ctx, next) =>
{
    var prefix =
        ctx.Request.Headers["X-Forwarded-Prefix"].FirstOrDefault()
        ?? ctx.Request.Headers["X-Ingress-Path"].FirstOrDefault();

    if (!string.IsNullOrEmpty(prefix))
    {
        // Sätt PathBase så att /api/hassio_ingress/<token> skalas av
        ctx.Request.PathBase = prefix;
    }

    return next();
});

app.UseStaticFiles();


app.MapGet("/health", () => Results.Ok(new { ok = true, time = DateTimeOffset.UtcNow }));
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
