using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Buttercup.Email;

public sealed class ServiceCollectionExtensionsTests
{
    private const string FakeFromAddress = "fake-from@example.com";

    #region AddEmailServices

    [Fact]
    public void AddEmailServices_AddsAzureEmailSender() =>
        Assert.Contains(
            new ServiceCollection().AddEmailServices(ConfigureOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IEmailSender) &&
                serviceDescriptor.ImplementationType == typeof(AzureEmailSender) &&
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

    private static void ConfigureOptions(EmailOptions options) =>
        options.FromAddress = FakeFromAddress;

    #endregion
}
