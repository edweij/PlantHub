using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using PlantHub.Web.Components;
using PlantHub.Web.Lib;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// ---- Read env/config ----
var sup = Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN")
          ?? Environment.GetEnvironmentVariable("HASSIO_TOKEN");

var cfg = builder.Configuration;
var baseUrl = cfg["HA:BaseUrl"] ?? Environment.GetEnvironmentVariable("PLANTHUB__BASEURL");
var token = cfg["HA:Token"] ?? Environment.GetEnvironmentVariable("PLANTHUB__TOKEN");

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var dataProtectionKeys = "/data/aspnet-keys"; // add-onens persistenta volym
Directory.CreateDirectory(dataProtectionKeys);

builder.Services
    .AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeys))
    .SetApplicationName("PlantHub"); // viktigt: stabilt namn över builds

builder.Services.AddAntiforgery(o =>
{
    o.Cookie.Name = "plh.af";
    o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

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

if (!string.IsNullOrWhiteSpace(sup))
{
    Console.WriteLine("[PlantHub] HA mode = Supervisor proxy (token present). Base= http://supervisor/core/api");
    // supervisor proxy base url is usually http://supervisor/core/api when running as addon
    builder.Services.AddSingleton<IHomeAssistantClient>(sp => new HomeAssistantSupervisorClient("http://supervisor/core/api", sup));
}
else if (!string.IsNullOrWhiteSpace(baseUrl) && !string.IsNullOrWhiteSpace(token))
{
    Console.WriteLine($"[PlantHub] HA mode = Direct (LLAT). Base= {baseUrl}");
    builder.Services.AddSingleton<IHomeAssistantClient>(sp => new HomeAssistantLlTokenClient(baseUrl, token));
}
else
{
    Console.WriteLine("[PlantHub] HA mode = Disabled (no token)");
    builder.Services.AddSingleton<IHomeAssistantClient, DisabledHomeAssistantClient>();
}

builder.WebHost.UseStaticWebAssets();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Läs ev. från env om du vill göra det konfigurerbart
var proxyIp = Environment.GetEnvironmentVariable("PLANTHUB__PROXY_IP") ?? "172.30.32.1";

// Om du föredrar ett helt subnet istället: "172.30.32.0/24"
var useSubnet = true;

var fwd = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor,

    // I HA/Ingress kan header-symmetri variera, gör det tolerant:
    RequireHeaderSymmetry = false,

    // Rimligt tak om du bara har en proxy framför dig.
    ForwardLimit = 2
};

if (useSubnet)
{
    // Tillåt hela hassio-nätet 172.30.32.0/24
    fwd.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("172.30.32.0"), 24));
}
else
{
    // Tillåt en specifik proxy
    fwd.KnownProxies.Add(IPAddress.Parse(proxyIp));
}

app.UseForwardedHeaders(fwd);

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

// --- Serve static files from wwwroot ---
// Flytta upp detta före antiforgery
app.UseStaticFiles();

// (valfritt) logga vad vi fick, vid felsökning
// app.Use(async (ctx, next) => {
//     Console.WriteLine($"PathBase='{ctx.Request.PathBase}', Path='{ctx.Request.Path}'");
//     await next();
// });

// --- Anti-forgery & resten ---
app.UseAntiforgery();

app.MapGet("/health", () => Results.Ok(new { ok = true, time = DateTimeOffset.UtcNow }));
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
