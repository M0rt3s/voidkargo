using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Game.Backend.Auth;
using Game.Backend.Entities;
using Game.Shared.Auth;
using Microsoft.Extensions.Options;

namespace Game.Backend.Tests;

public class JwtTokenServiceTests
{
    private static JwtTokenService CreateService(int expiryMinutes = 60) =>
        new(Options.Create(new JwtOptions
        {
            Key = "unit-test-signing-key-at-least-32-chars-long",
            Issuer = "voidkargo-tests",
            Audience = "voidkargo-tests-clients",
            ExpiryMinutes = expiryMinutes,
        }));

    private static UserEntity CreateUser() => new()
    {
        Id = Guid.NewGuid(),
        UserName = "player",
        Email = "player@voidkargo.local",
        DisplayName = "Dev Player",
    };

    [Fact]
    public void CreateToken_IncludesSubjectAndDisplayNameClaims()
    {
        var service = CreateService();
        var user = CreateUser();

        var (token, _) = service.CreateToken(user, new[] { GameRoles.Player });

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal(user.Id.ToString(), jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("Dev Player", jwt.Claims.Single(c => c.Type == "display_name").Value);
    }

    [Fact]
    public void CreateToken_IncludesAllSuppliedRolesAsClaims()
    {
        var service = CreateService();
        var user = CreateUser();

        var (token, _) = service.CreateToken(user, new[] { GameRoles.Admin, GameRoles.GameMaster });

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var roleClaims = jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();
        Assert.Contains(GameRoles.Admin, roleClaims);
        Assert.Contains(GameRoles.GameMaster, roleClaims);
    }

    [Fact]
    public void CreateToken_ExpiresAtUtcMatchesConfiguredExpiryMinutes()
    {
        var service = CreateService(expiryMinutes: 15);
        var user = CreateUser();

        var before = DateTimeOffset.UtcNow;
        var (_, expiresAtUtc) = service.CreateToken(user, new[] { GameRoles.Player });
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(expiresAtUtc, before.AddMinutes(15), after.AddMinutes(15).AddSeconds(1));
    }
}
