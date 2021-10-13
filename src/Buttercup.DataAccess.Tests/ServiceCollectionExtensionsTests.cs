using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Buttercup.DataAccess
{
    public class ServiceCollectionExtensionsTests
    {
        private const string ConnectionString = "connection-string";

        #region AddDataAccessServices

        [Fact]
        public void AddDataAccessServicesAddsConnectionSource() =>
            Assert.Contains(
                new ServiceCollection().AddDataAccessServices(ConfigureOptions),
                serviceDescriptor =>
                    serviceDescriptor.ServiceType == typeof(IDbConnectionSource) &&
                    serviceDescriptor.ImplementationType == typeof(DbConnectionSource) &&
                    serviceDescriptor.Lifetime == ServiceLifetime.Transient);

        [Fact]
        public void AddDataAccessServicesAddsAuthenticationEventDataProvider() =>
            Assert.Contains(
                new ServiceCollection().AddDataAccessServices(ConfigureOptions),
                serviceDescriptor =>
                    serviceDescriptor.ServiceType == typeof(IAuthenticationEventDataProvider) &&
                    serviceDescriptor.ImplementationType == typeof(AuthenticationEventDataProvider) &&
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
        public void AddDataAccessServicesConfiguresOptions()
        {
            var serviceProvider = new ServiceCollection()
                .AddDataAccessServices(ConfigureOptions)
                .BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<DataAccessOptions>>();

            Assert.Equal(ConnectionString, options.Value.ConnectionString);
        }

        [Fact]
        public void AddDataAccessServicesBindsConfiguration()
        {
            var configurationData = new Dictionary<string, string>()
            {
                ["ConnectionString"] = ConnectionString,
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationData)
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddDataAccessServices(configuration)
                .BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<DataAccessOptions>>();

            Assert.Equal(ConnectionString, options.Value.ConnectionString);
        }

        private static void ConfigureOptions(DataAccessOptions options) =>
            options.ConnectionString = ConnectionString;

        #endregion
    }
}
