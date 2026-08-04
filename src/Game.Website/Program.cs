using Game.Website.Components;
using Game.Website.Endpoints;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Aspire: service discovery (needed to resolve "https+http://game-backend" below), health
// checks, OpenTelemetry — see Game.Backend/Program.cs for the matching call.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Talks to Game.Backend's REST auth API (register/login/me) — see Endpoints/AccountEndpoints.cs.
// Base address is an Aspire service-discovery logical name, resolved via the WithReference(backend)
// wiring in Game.AppHost/AppHost.cs.
builder.Services.AddHttpClient("GameBackend", client =>
{
    client.BaseAddress = new Uri("https+http://game-backend");
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

// The website keeps its own cookie session; it never exposes the Backend-issued JWT to
// client-side script. The JWT travels only as a claim inside this server-side cookie
// principal, for future authenticated calls back to Game.Backend (and, per
// docs/01-architecture/system-architecture.md, eventually forwarded to the Unity client from
// the "Play" entry point).
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "voidkargo.auth";
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAccountEndpoints();

app.Run();
