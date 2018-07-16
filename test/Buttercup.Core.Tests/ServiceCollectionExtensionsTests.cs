using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Buttercup
{
    public class ServiceCollectionExtensionsTests
    {
        #region AddCoreServices

        [Fact]
        public void AddCoreServicesAddsClock()
        {
            var mockServiceCollection = new Mock<IServiceCollection>();

            mockServiceCollection.Object.AddCoreServices();

            mockServiceCollection.Verify(x => x.Add(It.Is<ServiceDescriptor>(serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IClock) &&
                serviceDescriptor.ImplementationType == typeof(Clock) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient)));
        }

        #endregion
    }
}
