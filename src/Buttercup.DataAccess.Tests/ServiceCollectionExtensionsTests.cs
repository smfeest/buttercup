using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Buttercup.DataAccess;

public sealed class ServiceCollectionExtensionsTests
{
    #region AddDataAccessServices

    [Fact]
    public void AddDataAccessServices_AddsRecipeDataProvider() =>
        Assert.Contains(
            new ServiceCollection().AddDataAccessServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IRecipeDataProvider) &&
                serviceDescriptor.ImplementationType == typeof(RecipeDataProvider) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    #endregion
}
