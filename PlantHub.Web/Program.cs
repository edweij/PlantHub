using PlantHub.Web.Components;
using PlantHub.Web.Lib;

var builder = WebApplication.CreateBuilder(args);

// ---- Read env/config ----
var cfg = builder.Configuration;
var supervisorToken = Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN");
var haBaseUrl = cfg["HA:BaseUrl"];   // e.g. http://homeassistant.local:8123/api
var haToken = cfg["HA:Token"];

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Minimal HA client DI
builder.Services.AddSingleton<IHomeAssistantClient>(_ =>
{
    if (!string.IsNullOrWhiteSpace(supervisorToken))
        return new HomeAssistantClient("http://supervisor/core/api", supervisorToken);
    if (!string.IsNullOrWhiteSpace(haBaseUrl) && !string.IsNullOrWhiteSpace(haToken))
        return new HomeAssistantClient(haBaseUrl, haToken);
    return new HomeAssistantClient(null, null); // disabled
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();

app.MapGet("/health", () => Results.Ok(new { ok = true, time = DateTimeOffset.UtcNow }));
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();


//using PlantHub.Web.Components;  // App
//using PlantHub.Web.Lib;         // IHomeAssistantClient, HomeAssistantClient

//var builder = WebApplication.CreateBuilder(args);



//// Razor Components (unified) + Interactive Server
//builder.Services.AddRazorComponents()
//    .AddInteractiveServerComponents();



//var app = builder.Build();

//// ---- Home Assistant Ingress PathBase support ----
//app.Use((ctx, next) =>
//{
//    if (ctx.Request.Headers.TryGetValue("X-INGRESS-ENTRY", out var prefix) && !string.IsNullOrEmpty(prefix))
//        ctx.Request.PathBase = prefix.ToString();
//    return next();
//});

//if (!app.Environment.IsDevelopment())
//    app.UseExceptionHandler("/Error");

//app.UseStaticFiles();
//app.UseAntiforgery();

//// Health endpoint for quick checks
//app.MapGet("/health", () => Results.Ok(new { ok = true, time = DateTimeOffset.UtcNow }));

//// Map root Razor component tree (no _Host.cshtml here)
//app.MapRazorComponents<App>()
//   .AddInteractiveServerRenderMode();

//app.Run();
