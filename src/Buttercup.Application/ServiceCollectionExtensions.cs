using Buttercup.Application.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Buttercup.Application;

/// <summary>
/// Extends <see cref="IServiceCollection" /> to facilitate the addition of application services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds application services to the service collection.
    /// </summary>
    /// <param name="services">
    /// The service collection.
    /// </param>
    /// <returns>
    /// The service collection to allow chaining.
    /// </returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services) =>
        services
            .AddTransient<IRecipeManager, RecipeManager>()
            .AddTransient<IUserManager, UserManager>()
            .AddTransient(typeof(IValidationErrorLocalizer<>), typeof(ValidationErrorLocalizer<>))
            .AddSingleton(typeof(IValidator<>), typeof(Validator<>));
}
