using Microsoft.Extensions.DependencyInjection;

namespace Buttercup.DataAccess;

/// <summary>
/// Extends <see cref="IServiceCollection" /> to facilitate the addition of data access services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds data access services to the service collection.
    /// </summary>
    /// <param name="services">
    /// The service collection.
    /// </param>
    /// <returns>
    /// The service collection to allow chaining.
    /// </returns>
    public static IServiceCollection AddDataAccessServices(this IServiceCollection services) =>
        services
            .AddTransient<IAuthenticationEventDataProvider, AuthenticationEventDataProvider>()
            .AddTransient<IPasswordResetTokenDataProvider, PasswordResetTokenDataProvider>()
            .AddTransient<IRecipeDataProvider, RecipeDataProvider>()
            .AddTransient<IUserDataProvider, UserDataProvider>();
}
