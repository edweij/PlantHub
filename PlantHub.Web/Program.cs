using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using PlantHub.Web.Components;
using PlantHub.Web.Lib;

var builder = WebApplication.CreateBuilder(args);

// ---- Read env/config ----
var cfg = builder.Configuration;
var supervisorToken = Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN");

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

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
    var supervisorToken = Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN");
    if (!string.IsNullOrWhiteSpace(supervisorToken))
        return new HomeAssistantClient("http://supervisor/core/api", supervisorToken);

    var cfg = builder.Configuration;
    var baseUrl = cfg["HA:BaseUrl"] ?? Environment.GetEnvironmentVariable("PLANTHUB__BASEURL");
    var token = cfg["HA:Token"] ?? Environment.GetEnvironmentVariable("PLANTHUB__TOKEN");

    if (!string.IsNullOrWhiteSpace(baseUrl) && !string.IsNullOrWhiteSpace(token))
        return new HomeAssistantClient(baseUrl, token);

    return new HomeAssistantClient(null, null);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// --- Apply forwarded headers (HA proxy / ingress) ---
app.UseForwardedHeaders();

// --- Handle HA Ingress path ---
app.Use((ctx, next) =>
{
    var ingressPath = ctx.Request.Headers["X-Ingress-Path"].FirstOrDefault();
    if (!string.IsNullOrEmpty(ingressPath))
    {
        ctx.Request.PathBase = ingressPath;
    }
    return next();
});

// --- Standard Blazor setup ---
app.UseAntiforgery();

var provider = new FileExtensionContentTypeProvider();
// Lägg till extra typer som HA ibland tappar
provider.Mappings[".css"] = "text/css";
provider.Mappings[".js"] = "application/javascript";
provider.Mappings[".json"] = "application/json";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider,
    ServeUnknownFileTypes = false // låt Kestrel hantera resten
});

// Optional health endpoint
app.MapGet("/health", () => Results.Ok(new { ok = true, time = DateTimeOffset.UtcNow }));

// Map Razor components (Blazor Server interactive)
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
