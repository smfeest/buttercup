using Buttercup.EntityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Buttercup.Security;

public sealed class ServiceCollectionExtensionsTests
{
    #region AddSecurityServices

    [Fact]
    public void AddSecurityServices_AddsPasswordHasher() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IPasswordHasher<User>) &&
                serviceDescriptor.ImplementationType == typeof(PasswordHasher<User>) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsAccessTokenEncoder() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IAccessTokenEncoder) &&
                serviceDescriptor.ImplementationType == typeof(AccessTokenEncoder) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsAccessTokenSerializer() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IAccessTokenSerializer) &&
                serviceDescriptor.ImplementationType == typeof(AccessTokenSerializer) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsAuthenticationMailer() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IAuthenticationMailer) &&
                serviceDescriptor.ImplementationType == typeof(AuthenticationMailer) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsCookieAuthenticationService() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(ICookieAuthenticationService) &&
                serviceDescriptor.ImplementationType == typeof(CookieAuthenticationService) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsPasswordAuthenticationService() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IPasswordAuthenticationService) &&
                serviceDescriptor.ImplementationType == typeof(PasswordAuthenticationService) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsRandomNumberGeneratorFactory() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IRandomNumberGeneratorFactory) &&
                serviceDescriptor.ImplementationType == typeof(RandomNumberGeneratorFactory) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsRandomTokenGenerator() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IRandomTokenGenerator) &&
                serviceDescriptor.ImplementationType == typeof(RandomTokenGenerator) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsTokenAuthenticationService() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(ITokenAuthenticationService) &&
                serviceDescriptor.ImplementationType == typeof(TokenAuthenticationService) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServices_AddsUserPrincipalFactory() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IUserPrincipalFactory) &&
                serviceDescriptor.ImplementationType == typeof(UserPrincipalFactory) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    #endregion
}
