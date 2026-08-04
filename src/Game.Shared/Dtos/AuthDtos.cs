// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
using System;
using System.Collections.Generic;

namespace Game.Shared.Dtos
{
    /// <summary>
    /// Self-registration request. Always results in a <see cref="Auth.GameRoles.Player"/>
    /// account — elevated roles (Admin, GameMaster) are never grantable through this endpoint,
    /// only via the Development seeder or a future admin-only management surface.
    /// </summary>
    public sealed record RegisterRequestDto(string UserName, string Email, string Password, string DisplayName);

    /// <summary>Credential submission for <c>POST /api/auth/login</c>.</summary>
    public sealed record LoginRequestDto(string UserName, string Password);

    /// <summary>
    /// Issued on successful login/register. <see cref="Token"/> is a signed JWT bearer token;
    /// see docs/01-architecture/networking-strategy.md for how it later flows into the Unity
    /// client via the Website's "Play" entry point.
    /// </summary>
    public sealed record AuthResponseDto(string Token, DateTimeOffset ExpiresAtUtc, UserProfileDto Profile);

    /// <summary>The authenticated user's public profile and role membership.</summary>
    public sealed record UserProfileDto(Guid Id, string UserName, string Email, string DisplayName, IReadOnlyList<string> Roles);
}
