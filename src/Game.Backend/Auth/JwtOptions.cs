namespace Game.Backend.Auth;

/// <summary>
/// JWT signing configuration, bound from the <c>Jwt</c> configuration section. In Development,
/// a placeholder <see cref="Key"/> ships in <c>appsettings.Development.json</c> for zero-friction
/// local runs; outside Development a real secret <b>must</b> be supplied via environment
/// variable/user-secrets/secret manager — see <see cref="JwtOptionsValidation"/>, which fails
/// startup fast rather than silently signing tokens with a known key.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>HMAC-SHA256 signing key. Must be at least 32 characters (256 bits).</summary>
    public required string Key { get; set; }

    public required string Issuer { get; set; }

    public required string Audience { get; set; }

    /// <summary>How long an issued access token remains valid.</summary>
    public int ExpiryMinutes { get; set; } = 60;
}
