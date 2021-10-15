using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Buttercup.Email
{
    public class ServiceCollectionExtensionsTests
    {
        #region AddEmailServices

        [Fact]
        public void AddEmailServicesAddsEmailSender() =>
            Assert.Contains(
                new ServiceCollection().AddEmailServices(options => { }),
                serviceDescriptor =>
                    serviceDescriptor.ServiceType == typeof(IEmailSender) &&
                    serviceDescriptor.ImplementationType == typeof(EmailSender) &&
                    serviceDescriptor.Lifetime == ServiceLifetime.Transient);

        [Fact]
        public void AddEmailServicesAddsSendGridClientAccessor() =>
            Assert.Contains(
                new ServiceCollection().AddEmailServices(options => { }),
                serviceDescriptor =>
                    serviceDescriptor.ServiceType == typeof(ISendGridClientAccessor) &&
                    serviceDescriptor.ImplementationType == typeof(SendGridClientAccessor) &&
                    serviceDescriptor.Lifetime == ServiceLifetime.Transient);

        [Fact]
        public void AddEmailServicesConfiguresOptions()
        {
            var serviceProvider = new ServiceCollection()
                .AddEmailServices(options => options.ApiKey = "api-key")
                .BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<EmailOptions>>();

            Assert.Equal("api-key", options.Value.ApiKey);
        }

        [Fact]
        public void AddEmailServicesBindsConfiguration()
        {
            var configurationData = new Dictionary<string, string>()
            {
                ["ApiKey"] = "api-key",
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationData)
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddEmailServices(configuration)
                .BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<EmailOptions>>();

            Assert.Equal("api-key", options.Value.ApiKey);
        }

        #endregion
    }
}
