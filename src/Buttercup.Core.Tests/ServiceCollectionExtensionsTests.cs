using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Buttercup;

public sealed class ServiceCollectionExtensionsTests
{
    #region AddCoreServices

    [Fact]
    public void AddCoreServices_AddsClock() =>
        Assert.Contains(
            new ServiceCollection().AddCoreServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IClock) &&
                serviceDescriptor.ImplementationType == typeof(Clock) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    #endregion
}
