using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Buttercup;

public class ServiceCollectionExtensionsTests
{
    #region AddCoreServices

    [Fact]
    public void AddCoreServicesAddsClock() =>
        Assert.Contains(
            new ServiceCollection().AddCoreServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IClock) &&
                serviceDescriptor.ImplementationType == typeof(Clock) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    #endregion
}
