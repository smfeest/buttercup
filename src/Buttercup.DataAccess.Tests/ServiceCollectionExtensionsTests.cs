using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Buttercup.DataAccess;

public sealed class ServiceCollectionExtensionsTests
{
    private const string ConnectionString = "connection-string";

    #region AddDataAccessServices

    [Fact]
    public void AddDataAccessServicesAddsAuthenticationEventDataProvider() =>
        Assert.Contains(
            new ServiceCollection().AddDataAccessServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IAuthenticationEventDataProvider) &&
                serviceDescriptor.ImplementationType == typeof(AuthenticationEventDataProvider) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddDataAccessServicesAddsPasswordResetTokenDataProvider() =>
        Assert.Contains(
            new ServiceCollection().AddDataAccessServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IPasswordResetTokenDataProvider) &&
                serviceDescriptor.ImplementationType == typeof(PasswordResetTokenDataProvider) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddDataAccessServicesAddsRecipeDataProvider() =>
        Assert.Contains(
            new ServiceCollection().AddDataAccessServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IRecipeDataProvider) &&
                serviceDescriptor.ImplementationType == typeof(RecipeDataProvider) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddDataAccessServicesAddsUserDataProvider() =>
        Assert.Contains(
            new ServiceCollection().AddDataAccessServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IUserDataProvider) &&
                serviceDescriptor.ImplementationType == typeof(UserDataProvider) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    #endregion
}
