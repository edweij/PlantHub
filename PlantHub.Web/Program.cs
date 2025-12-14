using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using PlantHub.Web.Components;
using PlantHub.Web.Components.PhToast;
using PlantHub.Web.Infrastructure;
using PlantHub.Web.Lib;
using System.Data.Common;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// ---- Read env/config ----
// Prefer supervisor token if present (HA add-on). Fallback to legacy HASSIO_TOKEN.
var sup = Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN")
          ?? Environment.GetEnvironmentVariable("HASSIO_TOKEN");

// Read config for direct LLAT mode (dev/local or external HA)
var cfg = builder.Configuration;
var baseUrl = cfg["HA:BaseUrl"] ?? Environment.GetEnvironmentVariable("PLANTHUB__BASEURL");
var token = cfg["HA:Token"] ?? Environment.GetEnvironmentVariable("PLANTHUB__TOKEN");

// ----------------------------------------------------
// Connection string (dev vs prod) + EF Core / SQLite
// ----------------------------------------------------
// In dev, keep DB in local working dir; in HA add-on, persist under /data.
var cs = builder.Configuration.GetConnectionString("PlantHub")
         ?? (builder.Environment.IsDevelopment()
                ? "Data Source=planthub.dev.db"
                : "Data Source=/data/planthub.db");

// Register DbContext with SQLite provider.
builder.Services.AddDbContext<PlantHubDbContext>(opt => opt.UseSqlite(cs));
builder.Services.AddScoped<IWateringGroupService, WateringGroupService>();
builder.Services.AddScoped<IPlantService, PlantService>();
builder.Services.AddScoped<PhToastService>();

if (!string.IsNullOrWhiteSpace(sup))
{
    Console.WriteLine("[PlantHub] Image storage = Addon mode (/config/www/...)");
    builder.Services.AddScoped<IImageStorageService, AddonImageStorageService>();
}
else
{
    Console.WriteLine("[PlantHub] Image storage = Local mode (wwwroot/local/...)");
    builder.Services.AddScoped<IImageStorageService, LocalImageStorageService>();
}

// ---- Notification service ----
builder.Services.Configure<NotificationSettings>(builder.Configuration.GetSection("Notifications"));
builder.Services.AddScoped<PlantHubNotificationService>();

// Add watering monitor service
builder.Services.AddScoped<WateringCheckService>();
builder.Services.AddHostedService<WateringMonitorService>();

// ---- UI stack (Blazor Server) ----
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Persist ASP.NET DataProtection keys so auth/antiforgery and cookies survive container restarts.
var dataProtectionKeys = "/data/aspnet-keys"; // add-on persistent volume
Directory.CreateDirectory(dataProtectionKeys);

builder.Services
    .AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeys))
    .SetApplicationName("PlantHub"); // must be stable across builds to reuse keys

// Antiforgery cookie tuning (short name and same security policy as request)
builder.Services.AddAntiforgery(o =>
{
    o.Cookie.Name = "plh.af";
    o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// --- Ingress / Reverse proxy fix ---
// We’ll accept forwarded headers and resolve the original scheme/host sent by HA proxy.
// We don’t pre-fill KnownProxies here; we’ll do it later with UseForwardedHeaders.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                               ForwardedHeaders.XForwardedProto |
                               ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddHttpContextAccessor();

// Blazor Server detailed errors (handy in dev; safe-ish behind HA ingress)
builder.Services.AddServerSideBlazor().AddCircuitOptions(options => options.DetailedErrors = true);

// ---- Home Assistant client mode selection ----
if (!string.IsNullOrWhiteSpace(sup))
{
    Console.WriteLine("[PlantHub] HA mode = Supervisor proxy (token present). Base= http://supervisor/core/api");
    // In add-on, talk to HA Core via supervisor proxy endpoint.
    builder.Services.AddSingleton<IHomeAssistantClient>(sp =>
        new HomeAssistantSupervisorClient("http://supervisor/core/api", sup));
}
else if (!string.IsNullOrWhiteSpace(baseUrl) && !string.IsNullOrWhiteSpace(token))
{
    Console.WriteLine($"[PlantHub] HA mode = Direct (LLAT). Base= {baseUrl}");
    builder.Services.AddSingleton<IHomeAssistantClient>(sp =>
        new HomeAssistantLlTokenClient(baseUrl, token));
}
else
{
    Console.WriteLine("[PlantHub] HA mode = Disabled (no token)");
    builder.Services.AddSingleton<IHomeAssistantClient, DisabledHomeAssistantClient>();
}

// Enable static web assets in dev (so wwwroot from referenced projects are served)
builder.WebHost.UseStaticWebAssets();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // In dev, redirect HTTP -> HTTPS (typically useful locally; behind HA usually not needed)
    app.UseHttpsRedirection();
}

// ---- Forwarded headers for HA ingress ----
// Read proxy IP/subnet from env; default to HA OS internal network.
var proxyIp = Environment.GetEnvironmentVariable("PLANTHUB__PROXY_IP") ?? "172.30.32.1";

// If you prefer allowing the whole hassio subnet rather than single IP, keep this true.
var useSubnet = true;

var fwd = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor,

    // HA ingress may not preserve header symmetry; be tolerant:
    RequireHeaderSymmetry = false,

    // Reasonable cap (one or two proxies are typical in HA).
    ForwardLimit = 2
};

if (useSubnet)
{
    // Allow the entire hassio network 172.30.32.0/24
    fwd.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("172.30.32.0"), 24));
}
else
{
    // Allow a specific proxy address only
    fwd.KnownProxies.Add(IPAddress.Parse(proxyIp));
}

app.UseForwardedHeaders(fwd);

// --- Handle HA Ingress path / reverse proxy prefix ---
// HA ingress injects a prefix (e.g. /api/hassio_ingress/<token>/...).
// Teach ASP.NET Core about it so routing/static files work under the prefixed path.
app.Use((ctx, next) =>
{
    var prefix =
        ctx.Request.Headers["X-Forwarded-Prefix"].FirstOrDefault()
        ?? ctx.Request.Headers["X-Ingress-Path"].FirstOrDefault();

    if (!string.IsNullOrEmpty(prefix))
    {
        // Set PathBase so downstream middleware and link generation use the correct base path
        ctx.Request.PathBase = prefix;
    }

    return next();
});

// Serve static files from wwwroot (Blazor, CSS, etc.)
app.UseStaticFiles();

// Add antiforgery. (Blazor Server uses this under the hood for form posts)
app.UseAntiforgery();

// --- Basic health endpoint ---
app.MapGet("/health", () => Results.Ok(new { ok = true, time = DateTimeOffset.UtcNow }));

// Map Blazor Server app
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// ------------------------------------
// Run EF Core migrations at startup
// ------------------------------------
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<PlantHubDbContext>();
        await db.Database.MigrateAsync();          // async variant är fin i top-level Program.cs
        logger.LogInformation("EF Core migrations applied.");
    }
    catch (Exception ex)
    {
        // I HA add-on/containers är det oftast bäst att faila hårt så Supervisor kan restart:a.
        logger.LogError(ex, "Failed to apply EF Core migrations at startup.");
        throw;
    }
}

// ------------------------------------
// Log the actual SQLite path in use
// ------------------------------------
app.Lifetime.ApplicationStarted.Register(() =>
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PlantHubDbContext>();
    DbConnection conn = db.Database.GetDbConnection();  // requires: using System.Data.Common;
    app.Logger.LogInformation("SQLite DataSource = {Path}", conn.DataSource);
});

app.Run();
