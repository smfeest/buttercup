using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Buttercup.DataAccess;

public class ServiceCollectionExtensionsTests
{
    private const string ConnectionString = "connection-string";

    #region AddDataAccessServices

    [Fact]
    public void AddDataAccessServicesAddsAuthenticationEventDataProvider() =>
        Assert.Contains(
            new ServiceCollection().AddDataAccessServices(ConfigureOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IAuthenticationEventDataProvider) &&
                serviceDescriptor.ImplementationType == typeof(AuthenticationEventDataProvider) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddDataAccessServicesAddsMySqlConnectionSource() =>
        Assert.Contains(
            new ServiceCollection().AddDataAccessServices(ConfigureOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IMySqlConnectionSource) &&
                serviceDescriptor.ImplementationType == typeof(MySqlConnectionSource) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddDataAccessServicesAddsPasswordResetTokenDataProvider() =>
        Assert.Contains(
            new ServiceCollection().AddDataAccessServices(ConfigureOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IPasswordResetTokenDataProvider) &&
                serviceDescriptor.ImplementationType == typeof(PasswordResetTokenDataProvider) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddDataAccessServicesAddsRecipeDataProvider() =>
        Assert.Contains(
            new ServiceCollection().AddDataAccessServices(ConfigureOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IRecipeDataProvider) &&
                serviceDescriptor.ImplementationType == typeof(RecipeDataProvider) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddDataAccessServicesAddsUserDataProvider() =>
        Assert.Contains(
            new ServiceCollection().AddDataAccessServices(ConfigureOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IUserDataProvider) &&
                serviceDescriptor.ImplementationType == typeof(UserDataProvider) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddDataAccessServicesWithConfigureActionConfiguresOptions()
    {
        var options = new ServiceCollection()
            .AddDataAccessServices(ConfigureOptions)
            .BuildServiceProvider()
            .GetRequiredService<IOptions<DataAccessOptions>>();

        Assert.Equal(ConnectionString, options.Value.ConnectionString);
    }

    [Fact]
    public void AddDataAccessServicesWithConfigurationBindsConfiguration()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>()
            {
                ["ConnectionString"] = ConnectionString,
            })
            .Build();

        var options = new ServiceCollection()
            .AddDataAccessServices(configuration)
            .BuildServiceProvider()
            .GetRequiredService<IOptions<DataAccessOptions>>();

        Assert.Equal(ConnectionString, options.Value.ConnectionString);
    }

    [Fact]
    public void AddDataAccessServicesValidatesOptions()
    {
        var options = new ServiceCollection()
            .AddDataAccessServices(options => options.ConnectionString = string.Empty)
            .BuildServiceProvider()
            .GetRequiredService<IOptions<DataAccessOptions>>();

        Assert.Throws<OptionsValidationException>(() => options.Value);
    }

    private static void ConfigureOptions(DataAccessOptions options) =>
        options.ConnectionString = ConnectionString;

    #endregion
}
