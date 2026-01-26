using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Buttercup.Security;

public sealed class ServiceCollectionExtensionsTests
{
    private static readonly KeyValuePair<string, string?>[] ConfigValues =
    [
        new("Security:PasswordAuthenticationRateLimit:Limit", "1"),
        new("Security:PasswordAuthenticationRateLimit:Window", "00:00:00.100"),
        new("Security:PasswordResetRateLimits:Global:Limit", "2"),
        new("Security:PasswordResetRateLimits:Global:Window", "00:00:00.200"),
        new("Security:PasswordResetRateLimits:PerEmail:Limit", "3"),
        new("Security:PasswordResetRateLimits:PerEmail:Window", "00:00:00.300"),
    ];

    #region AddSecurityServices

    [Fact]
    public void AddSecurityServices_AddsPasswordHasher() =>
        Assert.Contains(
            new ServiceCollection().AddInMemoryConfiguration(ConfigValues).AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IPasswordHasher<User>) &&
                serviceDescriptor.ImplementationType == typeof(PasswordHasher<User>) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsAccessTokenEncoder() =>
        Assert.Contains(
            new ServiceCollection().AddInMemoryConfiguration(ConfigValues).AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IAccessTokenEncoder) &&
                serviceDescriptor.ImplementationType == typeof(AccessTokenEncoder) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsAccessTokenSerializer() =>
        Assert.Contains(
            new ServiceCollection().AddInMemoryConfiguration(ConfigValues).AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IAccessTokenSerializer) &&
                serviceDescriptor.ImplementationType == typeof(AccessTokenSerializer) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsAuthenticationMailer() =>
        Assert.Contains(
            new ServiceCollection().AddInMemoryConfiguration(ConfigValues).AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IAuthenticationMailer) &&
                serviceDescriptor.ImplementationType == typeof(AuthenticationMailer) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsClaimsIdentityFactory() =>
        Assert.Contains(
            new ServiceCollection().AddInMemoryConfiguration(ConfigValues).AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IClaimsIdentityFactory) &&
                serviceDescriptor.ImplementationType == typeof(ClaimsIdentityFactory) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsCookieAuthenticationService() =>
        Assert.Contains(
            new ServiceCollection().AddInMemoryConfiguration(ConfigValues).AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(ICookieAuthenticationService) &&
                serviceDescriptor.ImplementationType == typeof(CookieAuthenticationService) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsParameterMaskingService() =>
        Assert.Contains(
            new ServiceCollection().AddInMemoryConfiguration(ConfigValues).AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IParameterMaskingService) &&
                serviceDescriptor.ImplementationType == typeof(ParameterMaskingService) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsPasswordAuthenticationRateLimiter() =>
        Assert.Contains(
            new ServiceCollection().AddInMemoryConfiguration(ConfigValues).AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IPasswordAuthenticationRateLimiter) &&
                serviceDescriptor.ImplementationType == typeof(PasswordAuthenticationRateLimiter) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsPasswordAuthenticationService() =>
        Assert.Contains(
            new ServiceCollection().AddInMemoryConfiguration(ConfigValues).AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IPasswordAuthenticationService) &&
                serviceDescriptor.ImplementationType == typeof(PasswordAuthenticationService) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsPasswordResetRateLimiter() =>
        Assert.Contains(
            new ServiceCollection().AddInMemoryConfiguration(ConfigValues).AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IPasswordResetRateLimiter) &&
                serviceDescriptor.ImplementationType == typeof(PasswordResetRateLimiter) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsTokenAuthenticationService() =>
        Assert.Contains(
            new ServiceCollection().AddInMemoryConfiguration(ConfigValues).AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(ITokenAuthenticationService) &&
                serviceDescriptor.ImplementationType == typeof(TokenAuthenticationService) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_BindsSecurityOptions()
    {
        var options = new ServiceCollection()
            .AddInMemoryConfiguration(ConfigValues)
            .AddSecurityServices()
            .BuildServiceProvider()
            .GetRequiredService<IOptions<SecurityOptions>>();

        Assert.Equal(
            new()
            {
                PasswordAuthenticationRateLimit = new(1, 100),
                PasswordResetRateLimits = new()
                {
                    Global = new(2, 200),
                    PerEmail = new(3, 300),
                },
            },
            options.Value);
    }

    [Fact]
    public void AddSecurityServices_ValidatesSecurityOptions()
    {
        var configValues = new Dictionary<string, string?>(ConfigValues);
        configValues.Remove("Security:PasswordAuthenticationRateLimit:Limit");
        configValues.Remove("Security:PasswordAuthenticationRateLimit:Window");

        var options = new ServiceCollection()
            .AddInMemoryConfiguration(configValues)
            .AddSecurityServices()
            .BuildServiceProvider()
            .GetRequiredService<IOptions<SecurityOptions>>();

        Assert.Throws<OptionsValidationException>(() => options.Value);
    }

    #endregion
}
