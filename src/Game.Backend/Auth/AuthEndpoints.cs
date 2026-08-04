using System.Security.Claims;
using Game.Backend.Entities;
using Game.Shared.Auth;
using Game.Shared.Dtos;
using Microsoft.AspNetCore.Identity;

namespace Game.Backend.Auth;

/// <summary>REST auth endpoints: register, login, and the current-user profile.</summary>
public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (
            RegisterRequestDto request,
            UserManager<UserEntity> userManager,
            JwtTokenService tokenService) =>
        {
            var user = new UserEntity
            {
                UserName = request.UserName,
                Email = request.Email,
                DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? request.UserName : request.DisplayName,
            };

            var createResult = await userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                return Results.ValidationProblem(ToValidationErrors(createResult));
            }

            // Self-registration only ever grants Player — Admin/GameMaster accounts are
            // seeded in Development or, in future, granted via an admin-only management
            // surface. Never derived from client input.
            await userManager.AddToRoleAsync(user, GameRoles.Player);

            var (token, expiresAtUtc) = tokenService.CreateToken(user, new[] { GameRoles.Player });
            var profile = new UserProfileDto(user.Id, user.UserName!, user.Email!, user.DisplayName, new[] { GameRoles.Player });

            return Results.Ok(new AuthResponseDto(token, expiresAtUtc, profile));
        })
        .WithName("Register");

        group.MapPost("/login", async (
            LoginRequestDto request,
            UserManager<UserEntity> userManager,
            JwtTokenService tokenService) =>
        {
            var user = await userManager.FindByNameAsync(request.UserName);
            if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            {
                return Results.Unauthorized();
            }

            var roles = await userManager.GetRolesAsync(user);
            var (token, expiresAtUtc) = tokenService.CreateToken(user, roles);
            var profile = new UserProfileDto(user.Id, user.UserName!, user.Email!, user.DisplayName, roles.ToArray());

            return Results.Ok(new AuthResponseDto(token, expiresAtUtc, profile));
        })
        .WithName("Login");

        group.MapGet("/me", (ClaimsPrincipal principal) =>
        {
            var id = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userName = principal.Identity?.Name ?? string.Empty;
            var email = principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            var displayName = principal.FindFirstValue("display_name") ?? userName;
            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

            return Results.Ok(new UserProfileDto(id, userName, email, displayName, roles));
        })
        .RequireAuthorization()
        .WithName("Me");

        return app;
    }

    private static Dictionary<string, string[]> ToValidationErrors(IdentityResult result) =>
        result.Errors
            .GroupBy(e => e.Code)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
}
