using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Buttercup.Application;

public sealed class ServiceCollectionExtensionsTests
{
    #region AddApplicationServices

    [Fact]
    public void AddApplicationServices_AddsUserManager() =>
        Assert.Contains(
            new ServiceCollection().AddApplicationServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IUserManager) &&
                serviceDescriptor.ImplementationType == typeof(UserManager) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    #endregion
}
