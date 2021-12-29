using Microsoft.Extensions.Configuration;
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
    /// <param name="configure">
    /// An action that configures the data access options.
    /// </param>
    /// <returns>
    /// The service collection to allow chaining.
    /// </returns>
    public static IServiceCollection AddDataAccessServices(
        this IServiceCollection services, Action<DataAccessOptions> configure) =>
        services.Configure(configure).AddDataAccessServices();

    /// <summary>
    /// Adds data access services to the service collection.
    /// </summary>
    /// <param name="services">
    /// The service collection.
    /// </param>
    /// <param name="configuration">
    /// The data access configuration.
    /// </param>
    /// <returns>
    /// The service collection to allow chaining.
    /// </returns>
    public static IServiceCollection AddDataAccessServices(
        this IServiceCollection services, IConfiguration configuration) =>
        services.Configure<DataAccessOptions>(configuration).AddDataAccessServices();

    private static IServiceCollection AddDataAccessServices(this IServiceCollection services) =>
        services
            .AddTransient<IAuthenticationEventDataProvider, AuthenticationEventDataProvider>()
            .AddTransient<IMySqlConnectionSource, MySqlConnectionSource>()
            .AddTransient<IPasswordResetTokenDataProvider, PasswordResetTokenDataProvider>()
            .AddTransient<IRecipeDataProvider, RecipeDataProvider>()
            .AddTransient<IUserDataProvider, UserDataProvider>();
}
