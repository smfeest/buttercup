using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MySql.Data.MySqlClient;
using Xunit;

namespace Buttercup.DataAccess
{
    public class ServiceCollectionExtensionsTests
    {
        #region AddDataAccessServices

        [Fact]
        public void AddDataAccessServicesAddsConnectionSource()
        {
            var mockServiceCollection = new Mock<IServiceCollection>();

            Func<IServiceProvider, object> instanceFactory = null;

            mockServiceCollection
                .Setup(x => x.Add(It.Is<ServiceDescriptor>(serviceDescriptor =>
                    serviceDescriptor.ServiceType == typeof(IDbConnectionSource) &&
                    serviceDescriptor.Lifetime == ServiceLifetime.Transient)))
                .Callback<ServiceDescriptor>(serviceDescriptor =>
                    instanceFactory = serviceDescriptor.ImplementationFactory).Verifiable();

            mockServiceCollection.Object.AddDataAccessServices("sample-connection-string");

            mockServiceCollection.Verify();

            var connectionSource = (DbConnectionSource)instanceFactory(null);

            Assert.Equal("sample-connection-string", connectionSource.ConnectionString);
        }

        [Fact]
        public void AddDataAccessServicesAddsRecipeDataProvider()
        {
            var mockServiceCollection = new Mock<IServiceCollection>();

            mockServiceCollection.Object.AddDataAccessServices("sample-connection-string");

            mockServiceCollection.Verify(x => x.Add(It.Is<ServiceDescriptor>(serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IRecipeDataProvider) &&
                serviceDescriptor.ImplementationType == typeof(RecipeDataProvider) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient)));
        }

        #endregion
    }
}
