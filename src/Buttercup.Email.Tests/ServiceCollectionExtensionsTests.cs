using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Buttercup.Email;

public sealed class ServiceCollectionExtensionsTests
{
    private const string FakeApiKey = "fake-api-key";
    private const string FakeFromAddress = "fake-from@example.com";

    #region AddEmailServices

    [Fact]
    public void AddEmailServices_AddsEmailSender()
    {
        var collection = new ServiceCollection().AddEmailServices(ConfigureOptions);

        Assert.Contains(
            collection,
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IEmailSender) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

        Assert.IsType<EmailSender>(
            collection.BuildServiceProvider().GetRequiredService<IEmailSender>());
    }

    [Fact]
    public void AddEmailServices_WithConfigureActionConfiguresOptions()
    {
        var options = new ServiceCollection()
            .AddEmailServices(ConfigureOptions)
            .BuildServiceProvider()
            .GetRequiredService<IOptions<EmailOptions>>();

        Assert.Equal(FakeApiKey, options.Value.ApiKey);
        Assert.Equal(FakeFromAddress, options.Value.FromAddress);
    }

    [Fact]
    public void AddEmailServices_WithConfigurationBindsConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                ["ApiKey"] = FakeApiKey,
                ["FromAddress"] = FakeFromAddress,
            })
            .Build();

        var options = new ServiceCollection()
            .AddEmailServices(configuration)
            .BuildServiceProvider()
            .GetRequiredService<IOptions<EmailOptions>>();

        Assert.Equal(FakeApiKey, options.Value.ApiKey);
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

    private static void ConfigureOptions(EmailOptions options)
    {
        options.ApiKey = FakeApiKey;
        options.FromAddress = FakeFromAddress;
    }

    #endregion
}
