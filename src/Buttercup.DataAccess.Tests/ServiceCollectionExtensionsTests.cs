using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Buttercup.DataAccess;

public sealed class ServiceCollectionExtensionsTests
{
    #region AddDataAccessServices

    [Fact]
    public void AddDataAccessServices_AddsAuthenticationEventDataProvider() =>
        Assert.Contains(
            new ServiceCollection().AddDataAccessServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IAuthenticationEventDataProvider) &&
                serviceDescriptor.ImplementationType == typeof(AuthenticationEventDataProvider) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddDataAccessServices_AddsPasswordResetTokenDataProvider() =>
        Assert.Contains(
            new ServiceCollection().AddDataAccessServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IPasswordResetTokenDataProvider) &&
                serviceDescriptor.ImplementationType == typeof(PasswordResetTokenDataProvider) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddDataAccessServices_AddsRecipeDataProvider() =>
        Assert.Contains(
            new ServiceCollection().AddDataAccessServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IRecipeDataProvider) &&
                serviceDescriptor.ImplementationType == typeof(RecipeDataProvider) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddDataAccessServices_AddsUserDataProvider() =>
        Assert.Contains(
            new ServiceCollection().AddDataAccessServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IUserDataProvider) &&
                serviceDescriptor.ImplementationType == typeof(UserDataProvider) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    #endregion
}
