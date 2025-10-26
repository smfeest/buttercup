using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Buttercup.Email;

public sealed class ServiceCollectionExtensionsTests
{
    private const string FakeFromAddress = "fake-from@example.com";

    #region AddEmailServices

    [Fact]
    public void AddEmailServices_AddsEmailSenderFactory() =>
        Assert.Contains(
            new ServiceCollection().AddEmailServices(ConfigureOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IEmailSender) &&
                serviceDescriptor.ImplementationFactory is not null &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddEmailServices_WithConfigureActionConfiguresOptions()
    {
        var options = new ServiceCollection()
            .AddEmailServices(ConfigureOptions)
            .BuildServiceProvider()
            .GetRequiredService<IOptions<EmailOptions>>();

        Assert.Equal(FakeFromAddress, options.Value.FromAddress);
    }

    [Fact]
    public void AddEmailServices_WithConfigurationBindsConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                ["FromAddress"] = FakeFromAddress,
            })
            .Build();

        var options = new ServiceCollection()
            .AddEmailServices(configuration)
            .BuildServiceProvider()
            .GetRequiredService<IOptions<EmailOptions>>();

        Assert.Equal(FakeFromAddress, options.Value.FromAddress);
    }

    [Fact]
    public void AddEmailServices_ValidatesOptions()
    {
        var options = new ServiceCollection()
            .AddEmailServices(options => { })
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
            .AddTransient((_) => Mock.Of<EmailClient>())
            .AddEmailServices(options =>
            {
                ConfigureOptions(options);
                options.Provider = provider;
            })
            .BuildServiceProvider();

        Assert.IsType(expectedType, serviceProvider.GetService<IEmailSender>());
    }

    [Fact]
    public void EmailSenderFactory_ThrowsIfProviderInvalid()
    {
        var serviceProvider = new ServiceCollection()
            .AddEmailServices(options =>
            {
                ConfigureOptions(options);
                options.Provider = (EmailProvider)5;
            })
            .BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(
            () => serviceProvider.GetService<IEmailSender>());
        Assert.Equal("'5' is not a valid EmailProvider value", exception.Message);
    }

    private static void ConfigureOptions(EmailOptions options) =>
        options.FromAddress = FakeFromAddress;

    #endregion
}
