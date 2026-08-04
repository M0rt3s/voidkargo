using Game.Backend.Auth;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Game.Backend.Tests;

public class JwtOptionsValidationTests
{
    private sealed class FakeHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Game.Backend.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    private static JwtOptionsValidation CreateValidator(string environmentName) =>
        new(new FakeHostEnvironment(environmentName));

    [Fact]
    public void Validate_FailsWhenKeyMissing()
    {
        var validator = CreateValidator(Environments.Development);
        var options = new JwtOptions { Key = "", Issuer = "voidkargo", Audience = "voidkargo-clients" };

        var result = validator.Validate(null, options);

        Assert.True(result.Failed);
    }

    [Fact]
    public void Validate_FailsWhenKeyTooShort()
    {
        var validator = CreateValidator(Environments.Development);
        var options = new JwtOptions { Key = "too-short", Issuer = "voidkargo", Audience = "voidkargo-clients" };

        var result = validator.Validate(null, options);

        Assert.True(result.Failed);
    }

    [Fact]
    public void Validate_SucceedsInDevelopmentWithPlaceholderKey()
    {
        var validator = CreateValidator(Environments.Development);
        var options = new JwtOptions
        {
            Key = JwtOptionsValidation.DevelopmentOnlyPlaceholderKey,
            Issuer = "voidkargo",
            Audience = "voidkargo-clients",
        };

        var result = validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_FailsInProductionWithDevelopmentPlaceholderKey()
    {
        var validator = CreateValidator(Environments.Production);
        var options = new JwtOptions
        {
            Key = JwtOptionsValidation.DevelopmentOnlyPlaceholderKey,
            Issuer = "voidkargo",
            Audience = "voidkargo-clients",
        };

        var result = validator.Validate(null, options);

        Assert.True(result.Failed);
    }

    [Fact]
    public void Validate_FailsWhenIssuerOrAudienceMissing()
    {
        var validator = CreateValidator(Environments.Development);
        var options = new JwtOptions
        {
            Key = "a-sufficiently-long-signing-key-for-tests",
            Issuer = "",
            Audience = "voidkargo-clients",
        };

        var result = validator.Validate(null, options);

        Assert.True(result.Failed);
    }
}
