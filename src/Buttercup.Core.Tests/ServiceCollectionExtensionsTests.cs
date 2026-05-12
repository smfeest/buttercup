using System.Security.Cryptography;
using Buttercup.TestUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Buttercup;

public sealed class ServiceCollectionExtensionsTests
{
    private static readonly KeyValuePair<string, string?>[] ConfigValues =
        [new("Globalization:DefaultUserTimeZone", "fake-time-zone")];

    #region AddCoreServices

    [Fact]
    public void AddCoreServices_AddsRandomNumberGenerator()
    {
        var serviceCollection =
            new ServiceCollection().AddInMemoryConfiguration(ConfigValues).AddCoreServices();

        Assert.Contains(
            serviceCollection,
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(RandomNumberGenerator) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Singleton);

        using var serviceProvider = serviceCollection.BuildServiceProvider();

        serviceProvider.GetRequiredService<RandomNumberGenerator>();
    }

    [Fact]
    public void AddCoreServices_AddsRandomTokenGenerator() =>
        Assert.Contains(
            new ServiceCollection().AddInMemoryConfiguration(ConfigValues).AddCoreServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IRandomTokenGenerator) &&
                serviceDescriptor.ImplementationType == typeof(RandomTokenGenerator) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddCoreServices_BindsGlobalizationOptions()
    {
        var options = new ServiceCollection()
            .AddInMemoryConfiguration(ConfigValues)
            .AddCoreServices()
            .BuildServiceProvider()
            .GetRequiredService<IOptions<GlobalizationOptions>>();

        Assert.Equal(
            new() { DefaultUserTimeZone = "fake-time-zone" },
            options.Value);
    }

    [Fact]
    public void AddCoreServices_ValidatesGlobalizationOptions()
    {
        var configValues = new Dictionary<string, string?>(ConfigValues);
        configValues.Remove("Globalization:DefaultUserTimeZone");

        var options = new ServiceCollection()
            .AddInMemoryConfiguration(configValues)
            .AddCoreServices()
            .BuildServiceProvider()
            .GetRequiredService<IOptions<GlobalizationOptions>>();

        Assert.Throws<OptionsValidationException>(() => options.Value);
    }

    #endregion
}
