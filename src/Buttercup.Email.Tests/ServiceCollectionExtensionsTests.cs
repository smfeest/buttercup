using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Buttercup.Email
{
    public class ServiceCollectionExtensionsTests
    {
        #region AddEmailServices

        [Fact]
        public void AddEmailServicesAddsEmailSender() =>
            Assert.Contains(
                new ServiceCollection().AddEmailServices(),
                serviceDescriptor =>
                    serviceDescriptor.ServiceType == typeof(IEmailSender) &&
                    serviceDescriptor.ImplementationType == typeof(EmailSender) &&
                    serviceDescriptor.Lifetime == ServiceLifetime.Transient);

        [Fact]
        public void AddEmailServicesAddsSendGridClientAccessor() =>
            Assert.Contains(
                new ServiceCollection().AddEmailServices(),
                serviceDescriptor =>
                    serviceDescriptor.ServiceType == typeof(ISendGridClientAccessor) &&
                    serviceDescriptor.ImplementationType == typeof(SendGridClientAccessor) &&
                    serviceDescriptor.Lifetime == ServiceLifetime.Transient);

        #endregion
    }
}
