using Microsoft.AspNetCore.Identity;

namespace Game.Backend.Entities;

/// <summary>
/// ASP.NET Core Identity user for authentication. Deliberately separate from
/// <see cref="PlayerEntity"/>: this is the auth/account identity (username, password hash,
/// roles); <see cref="PlayerEntity"/> is the in-game profile (cash, fleet ownership). A future
/// migration can link the two via a nullable <c>PlayerEntity.OwnerUserId</c> once account
/// linking is needed — see docs/01-architecture/data-model.md.
/// </summary>
public sealed class UserEntity : IdentityUser<Guid>
{
    /// <summary>Public display name shown in the UI, separate from the Identity <c>UserName</c>.</summary>
    public required string DisplayName { get; set; }
}
