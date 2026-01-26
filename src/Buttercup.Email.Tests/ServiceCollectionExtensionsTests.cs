using Azure.Communication.Email;
using Buttercup.TestUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Buttercup.Email;

public sealed class ServiceCollectionExtensionsTests
{
    private static readonly KeyValuePair<string, string?>[] ConfigValues =
        [new("Email:FromAddress", "fake-from@example.com")];

    #region AddEmailServices

    [Fact]
    public void AddEmailServices_AddsEmailSenderFactory() =>
        Assert.Contains(
            new ServiceCollection().AddInMemoryConfiguration(ConfigValues).AddEmailServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IEmailSender) &&
                serviceDescriptor.ImplementationFactory is not null &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddEmailServices_BindsEmailOptions()
    {
        var options = new ServiceCollection()
            .AddInMemoryConfiguration(ConfigValues)
            .AddEmailServices()
            .BuildServiceProvider()
            .GetRequiredService<IOptions<EmailOptions>>();

        Assert.Equal(new() { FromAddress = "fake-from@example.com" }, options.Value);
    }

    [Fact]
    public void AddEmailServices_ValidatesEmailOptions()
    {
        var options = new ServiceCollection()
            .AddInMemoryConfiguration([new("Email:FromAddress", string.Empty)])
            .AddEmailServices()
            .BuildServiceProvider()
            .GetRequiredService<IOptions<EmailOptions>>();

        Assert.Throws<OptionsValidationException>(() => options.Value);
    }

    [Theory]
    [InlineData(EmailProvider.Azure, typeof(AzureEmailSender))]
    [InlineData(EmailProvider.Mailpit, typeof(MailpitSender))]
    public void EmailSenderFactory_ProvidesExpectedInstanceType(
        EmailProvider provider, Type expectedType)
    {
        var serviceProvider = new ServiceCollection()
            .AddInMemoryConfiguration([.. ConfigValues, new("Email:Provider", provider.ToString())])
            .AddTransient((_) => Mock.Of<EmailClient>())
            .AddEmailServices()
            .BuildServiceProvider();

        Assert.IsType(expectedType, serviceProvider.GetService<IEmailSender>());
    }

    [Fact]
    public void EmailSenderFactory_ThrowsIfProviderInvalid()
    {
        var serviceProvider = new ServiceCollection()
            .AddInMemoryConfiguration([.. ConfigValues, new("Email:Provider", "5")])
            .AddEmailServices()
            .BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(
            () => serviceProvider.GetService<IEmailSender>());
        Assert.Equal("'5' is not a valid EmailProvider value", exception.Message);
    }

    #endregion
}
