using Microsoft.Extensions.Options;

namespace Game.Backend.Auth;

/// <summary>
/// Fails application startup fast if <see cref="JwtOptions"/> is missing or, outside
/// Development, still set to the well-known local placeholder key shipped in
/// <c>appsettings.Development.json</c>. Prevents the "it silently signs prod tokens with a
/// public dev secret" misconfiguration risk called out in the task brief.
/// </summary>
public sealed class JwtOptionsValidation(IHostEnvironment environment) : IValidateOptions<JwtOptions>
{
    /// <summary>
    /// The exact Development-only placeholder from appsettings.Development.json. Never used
    /// as a real signing key outside Development — see <see cref="Validate"/>.
    /// </summary>
    public const string DevelopmentOnlyPlaceholderKey =
        "local-development-only-signing-key-do-not-use-in-production";

    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Key) || options.Key.Length < 32)
        {
            return ValidateOptionsResult.Fail(
                "Jwt:Key must be configured and at least 32 characters. Set it via environment " +
                "variable (Jwt__Key), user-secrets, or your secret manager — never commit a real " +
                "signing key to source.");
        }

        if (!environment.IsDevelopment() && options.Key == DevelopmentOnlyPlaceholderKey)
        {
            return ValidateOptionsResult.Fail(
                "Jwt:Key is still the Development-only placeholder outside the Development " +
                "environment. Configure a real secret via environment variable/secret manager " +
                "before running in this environment.");
        }

        if (string.IsNullOrWhiteSpace(options.Issuer) || string.IsNullOrWhiteSpace(options.Audience))
        {
            return ValidateOptionsResult.Fail("Jwt:Issuer and Jwt:Audience must both be configured.");
        }

        return ValidateOptionsResult.Success;
    }
}
