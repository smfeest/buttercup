using Buttercup.EntityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Buttercup.Security;

public sealed class ServiceCollectionExtensionsTests
{
    #region AddSecurityServices

    [Fact]
    public void AddSecurityServicesAddsPasswordHasher() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IPasswordHasher<User>) &&
                serviceDescriptor.ImplementationType == typeof(PasswordHasher<User>) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServicesAddsAccessTokenEncoder() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IAccessTokenEncoder) &&
                serviceDescriptor.ImplementationType == typeof(AccessTokenEncoder) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServicesAddsAccessTokenSerializer() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IAccessTokenSerializer) &&
                serviceDescriptor.ImplementationType == typeof(AccessTokenSerializer) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServicesAddsAuthenticationMailer() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IAuthenticationMailer) &&
                serviceDescriptor.ImplementationType == typeof(AuthenticationMailer) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServicesAddsCookieAuthenticationService() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(ICookieAuthenticationService) &&
                serviceDescriptor.ImplementationType == typeof(CookieAuthenticationService) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServicesAddsPasswordAuthenticationService() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IPasswordAuthenticationService) &&
                serviceDescriptor.ImplementationType == typeof(PasswordAuthenticationService) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServicesAddsRandomNumberGeneratorFactory() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IRandomNumberGeneratorFactory) &&
                serviceDescriptor.ImplementationType == typeof(RandomNumberGeneratorFactory) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServicesAddsRandomTokenGenerator() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IRandomTokenGenerator) &&
                serviceDescriptor.ImplementationType == typeof(RandomTokenGenerator) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServicesAddsTokenAuthenticationService() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(ITokenAuthenticationService) &&
                serviceDescriptor.ImplementationType == typeof(TokenAuthenticationService) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddSecurityServicesAddsUserPrincipalFactory() =>
        Assert.Contains(
            new ServiceCollection().AddSecurityServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IUserPrincipalFactory) &&
                serviceDescriptor.ImplementationType == typeof(UserPrincipalFactory) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    #endregion
}
