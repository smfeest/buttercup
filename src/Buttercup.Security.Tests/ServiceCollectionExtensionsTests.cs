using Buttercup.EntityModel;
using Buttercup.Redis.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Buttercup.Security;

public sealed class ServiceCollectionExtensionsTests
{
    private readonly SlidingWindowRateLimit passwordAuthenticationRateLimit = new(1, 100);

    #region AddSecurityServices

    [Fact]
    public void AddSecurityServices_AddsPasswordHasher() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(this.ConfigureOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IPasswordHasher<User>) &&
                serviceDescriptor.ImplementationType == typeof(PasswordHasher<User>) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsAccessTokenEncoder() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(this.ConfigureOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IAccessTokenEncoder) &&
                serviceDescriptor.ImplementationType == typeof(AccessTokenEncoder) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsAccessTokenSerializer() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(this.ConfigureOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IAccessTokenSerializer) &&
                serviceDescriptor.ImplementationType == typeof(AccessTokenSerializer) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsAuthenticationMailer() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(this.ConfigureOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IAuthenticationMailer) &&
                serviceDescriptor.ImplementationType == typeof(AuthenticationMailer) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsClaimsIdentityFactory() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(this.ConfigureOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IClaimsIdentityFactory) &&
                serviceDescriptor.ImplementationType == typeof(ClaimsIdentityFactory) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsCookieAuthenticationService() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(this.ConfigureOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(ICookieAuthenticationService) &&
                serviceDescriptor.ImplementationType == typeof(CookieAuthenticationService) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsParameterMaskingService() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(this.ConfigureOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IParameterMaskingService) &&
                serviceDescriptor.ImplementationType == typeof(ParameterMaskingService) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsPasswordAuthenticationRateLimiter() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(this.ConfigureOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IPasswordAuthenticationRateLimiter) &&
                serviceDescriptor.ImplementationType == typeof(PasswordAuthenticationRateLimiter) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsPasswordAuthenticationService() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(this.ConfigureOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IPasswordAuthenticationService) &&
                serviceDescriptor.ImplementationType == typeof(PasswordAuthenticationService) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsPasswordResetRateLimiter() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(this.ConfigureOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IPasswordResetRateLimiter) &&
                serviceDescriptor.ImplementationType == typeof(PasswordResetRateLimiter) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsTokenAuthenticationService() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(this.ConfigureOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(ITokenAuthenticationService) &&
                serviceDescriptor.ImplementationType == typeof(TokenAuthenticationService) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_WithConfigureActionConfiguresOptions()
    {
        var options = new ServiceCollection()
            .AddSecurityServices(this.ConfigureOptions)
            .BuildServiceProvider()
            .GetRequiredService<IOptions<SecurityOptions>>();

        Assert.Equal(
            this.passwordAuthenticationRateLimit,
            options.Value.PasswordAuthenticationRateLimit);
    }

    [Fact]
    public void AddSecurityServices_WithConfigurationBindsConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                ["PasswordAuthenticationRateLimit:Limit"] = "1",
                ["PasswordAuthenticationRateLimit:Window"] = "00:00:00.100",
                ["PasswordResetRateLimits:Global:Limit"] = "2",
                ["PasswordResetRateLimits:Global:Window"] = "00:00:00.200",
                ["PasswordResetRateLimits:PerEmail:Limit"] = "3",
                ["PasswordResetRateLimits:PerEmail:Window"] = "00:00:00.300",
            })
            .Build();

        var options = new ServiceCollection()
            .AddSecurityServices(configuration)
            .BuildServiceProvider()
            .GetRequiredService<IOptions<SecurityOptions>>();

        Assert.Equal(
            this.passwordAuthenticationRateLimit,
            options.Value.PasswordAuthenticationRateLimit);
    }

    [Fact]
    public void AddSecurityServices_ValidatesOptions()
    {
        var options = new ServiceCollection()
            .AddSecurityServices(options => { })
            .BuildServiceProvider()
            .GetRequiredService<IOptions<SecurityOptions>>();

        Assert.Throws<OptionsValidationException>(() => options.Value);
    }

    private void ConfigureOptions(SecurityOptions options)
    {
        options.PasswordAuthenticationRateLimit = this.passwordAuthenticationRateLimit;
        options.PasswordResetRateLimits = new() { Global = new(2, 200), PerEmail = new(3, 300) };
    }

    #endregion
}
