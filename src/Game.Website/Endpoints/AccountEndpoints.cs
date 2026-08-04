using System.Net.Http.Json;
using System.Security.Claims;
using Game.Shared.Dtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Game.Website.Endpoints;

/// <summary>
/// Non-interactive (plain HTML form POST) account endpoints. Deliberately minimal API
/// endpoints rather than Blazor `@onclick` handlers: signing a user in requires writing a
/// Set-Cookie response header, which can't happen once an interactive Blazor Server circuit's
/// response has already started streaming. See Components/Pages/Account/Login.razor and
/// Register.razor, which post plain forms here.
/// </summary>
public static class AccountEndpoints
{
    /// <summary>Claim type used to stash the Game.Backend-issued JWT on the website's own cookie
    /// principal, for later authenticated calls back to Game.Backend (never sent to the browser
    /// as anything other than this server-side cookie).</summary>
    public const string BackendTokenClaimType = "backend_jwt";

    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/account");

        group.MapPost("/login", (Delegate)HandleLoginAsync).WithName("AccountLogin");
        group.MapPost("/register", (Delegate)HandleRegisterAsync).WithName("AccountRegister");
        group.MapPost("/logout", (Delegate)HandleLogoutAsync).WithName("AccountLogout");

        return app;
    }

    private static async Task<IResult> HandleLoginAsync(
        HttpContext httpContext,
        IHttpClientFactory httpClientFactory,
        [FromForm] LoginFormModel form)
    {
        var client = httpClientFactory.CreateClient("GameBackend");
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto(form.UserName, form.Password));

        if (!response.IsSuccessStatusCode)
        {
            return RedirectWithError("/login", "invalid-credentials", form.ReturnUrl);
        }

        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (auth is null)
        {
            return RedirectWithError("/login", "unexpected-response", form.ReturnUrl);
        }

        await SignInAsync(httpContext, auth);

        return Results.LocalRedirect(NormalizeReturnUrl(form.ReturnUrl, "/profile"));
    }

    private static async Task<IResult> HandleRegisterAsync(
        HttpContext httpContext,
        IHttpClientFactory httpClientFactory,
        [FromForm] RegisterFormModel form)
    {
        if (form.Password != form.ConfirmPassword)
        {
            return RedirectWithError("/register", "password-mismatch", form.ReturnUrl);
        }

        var client = httpClientFactory.CreateClient("GameBackend");
        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequestDto(form.UserName, form.Email, form.Password, form.DisplayName));

        if (!response.IsSuccessStatusCode)
        {
            return RedirectWithError("/register", "registration-failed", form.ReturnUrl);
        }

        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (auth is null)
        {
            return RedirectWithError("/register", "unexpected-response", form.ReturnUrl);
        }

        // Auto-login immediately after successful registration.
        await SignInAsync(httpContext, auth);

        return Results.LocalRedirect(NormalizeReturnUrl(form.ReturnUrl, "/profile"));
    }

    private static async Task<IResult> HandleLogoutAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.LocalRedirect("/");
    }

    private static async Task SignInAsync(HttpContext httpContext, AuthResponseDto auth)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, auth.Profile.Id.ToString()),
            new(ClaimTypes.Name, auth.Profile.UserName),
            new(ClaimTypes.Email, auth.Profile.Email),
            new("display_name", auth.Profile.DisplayName),
            new(BackendTokenClaimType, auth.Token),
        };
        claims.AddRange(auth.Profile.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        // The website session's lifetime tracks the backend token's expiry — there's no
        // refresh-token flow yet, so once the JWT expires the user simply has to log in again.
        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = auth.ExpiresAtUtc,
        });
    }

    private static IResult RedirectWithError(string path, string errorCode, string? returnUrl)
    {
        var target = $"{path}?error={Uri.EscapeDataString(errorCode)}";
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            target += $"&returnUrl={Uri.EscapeDataString(returnUrl)}";
        }

        return Results.LocalRedirect(target);
    }

    /// <summary>Only ever redirects to a local, relative path — never trusts an absolute/external URL.</summary>
    private static string NormalizeReturnUrl(string? returnUrl, string fallback) =>
        !string.IsNullOrWhiteSpace(returnUrl) && returnUrl.StartsWith('/') && !returnUrl.StartsWith("//")
            ? returnUrl
            : fallback;

    private sealed class LoginFormModel
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }
    }

    private sealed class RegisterFormModel
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }
    }
}
