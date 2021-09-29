using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Buttercup.DataAccess
{
    public class ServiceCollectionExtensionsTests
    {
        private const string ConnectionString = "connection-string";

        #region AddDataAccessServices

        [Fact]
        public void AddDataAccessServicesAddsConnectionSource()
        {
            var serviceDescriptor = Assert.Single(
                new ServiceCollection().AddDataAccessServices(ConnectionString),
                descriptor => descriptor.ServiceType == typeof(IDbConnectionSource));

            Assert.Equal(ServiceLifetime.Transient, serviceDescriptor.Lifetime);

            var connectionSource = Assert.IsType<DbConnectionSource>(
                serviceDescriptor.ImplementationFactory(Mock.Of<IServiceProvider>()));

            Assert.Equal(ConnectionString, connectionSource.ConnectionString);
        }

        [Fact]
        public void AddDataAccessServicesAddsAuthenticationEventDataProvider() =>
            Assert.Contains(
                new ServiceCollection().AddDataAccessServices(ConnectionString),
                serviceDescriptor =>
                    serviceDescriptor.ServiceType == typeof(IAuthenticationEventDataProvider) &&
                    serviceDescriptor.ImplementationType == typeof(AuthenticationEventDataProvider) &&
                    serviceDescriptor.Lifetime == ServiceLifetime.Transient);

        [Fact]
        public void AddDataAccessServicesAddsPasswordResetTokenDataProvider() =>
            Assert.Contains(
                new ServiceCollection().AddDataAccessServices(ConnectionString),
                serviceDescriptor =>
                    serviceDescriptor.ServiceType == typeof(IPasswordResetTokenDataProvider) &&
                    serviceDescriptor.ImplementationType == typeof(PasswordResetTokenDataProvider) &&
                    serviceDescriptor.Lifetime == ServiceLifetime.Transient);

        [Fact]
        public void AddDataAccessServicesAddsRecipeDataProvider() =>
            Assert.Contains(
                new ServiceCollection().AddDataAccessServices(ConnectionString),
                serviceDescriptor =>
                    serviceDescriptor.ServiceType == typeof(IRecipeDataProvider) &&
                    serviceDescriptor.ImplementationType == typeof(RecipeDataProvider) &&
                    serviceDescriptor.Lifetime == ServiceLifetime.Transient);

        [Fact]
        public void AddDataAccessServicesAddsUserDataProvider() =>
            Assert.Contains(
                new ServiceCollection().AddDataAccessServices(ConnectionString),
                serviceDescriptor =>
                    serviceDescriptor.ServiceType == typeof(IUserDataProvider) &&
                    serviceDescriptor.ImplementationType == typeof(UserDataProvider) &&
                    serviceDescriptor.Lifetime == ServiceLifetime.Transient);

        #endregion
    }
}
