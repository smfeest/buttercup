using Buttercup.Application.Validation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Buttercup.Application;

public sealed class ServiceCollectionExtensionsTests
{
    #region AddApplicationServices

    [Fact]
    public void AddApplicationServices_AddsCommentManager() =>
        Assert.Contains(
            new ServiceCollection().AddApplicationServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(ICommentManager) &&
                serviceDescriptor.ImplementationType == typeof(CommentManager) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddApplicationServices_AddsRecipeManager() =>
        Assert.Contains(
            new ServiceCollection().AddApplicationServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IRecipeManager) &&
                serviceDescriptor.ImplementationType == typeof(RecipeManager) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddApplicationServices_AddsUserManager() =>
        Assert.Contains(
            new ServiceCollection().AddApplicationServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IUserManager) &&
                serviceDescriptor.ImplementationType == typeof(UserManager) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddApplicationServices_AddsValidationErrorLocalizer() =>
        Assert.Contains(
            new ServiceCollection().AddApplicationServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IValidationErrorLocalizer<>) &&
                serviceDescriptor.ImplementationType == typeof(ValidationErrorLocalizer<>) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddApplicationServices_AddsValidator() =>
        Assert.Contains(
            new ServiceCollection().AddApplicationServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IValidator<>) &&
                serviceDescriptor.ImplementationType == typeof(Validator<>) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Singleton);

    #endregion
}
