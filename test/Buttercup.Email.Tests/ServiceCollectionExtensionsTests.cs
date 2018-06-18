using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Buttercup.Email
{
    public class ServiceCollectionExtensionsTests
    {
        #region AddEmailServices

        [Fact]
        public void AddEmailServicesAddsEmailSender()
        {
            var mockServiceCollection = new Mock<IServiceCollection>();

            mockServiceCollection.Object.AddEmailServices();

            mockServiceCollection.Verify(x => x.Add(It.Is<ServiceDescriptor>(serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IEmailSender) &&
                serviceDescriptor.ImplementationType == typeof(EmailSender) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient)));
        }

        [Fact]
        public void AddEmailServicesAddsSendGridClientAccessor()
        {
            var mockServiceCollection = new Mock<IServiceCollection>();

            mockServiceCollection.Object.AddEmailServices();

            mockServiceCollection.Verify(x => x.Add(It.Is<ServiceDescriptor>(serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(ISendGridClientAccessor) &&
                serviceDescriptor.ImplementationType == typeof(SendGridClientAccessor) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient)));
        }

        #endregion
    }
}
