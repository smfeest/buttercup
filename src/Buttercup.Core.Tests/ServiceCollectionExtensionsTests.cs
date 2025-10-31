using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Buttercup;

public sealed class ServiceCollectionExtensionsTests
{
    #region AddCoreServices

    [Fact]
    public void AddCoreServices_AddsRandomNumberGeneratorFactory() =>
        Assert.Contains(
            new ServiceCollection().AddCoreServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IRandomNumberGeneratorFactory) &&
                serviceDescriptor.ImplementationType == typeof(RandomNumberGeneratorFactory) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    #endregion
}
