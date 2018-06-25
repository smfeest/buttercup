using Microsoft.Extensions.DependencyInjection;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// Extends <see cref="IServiceCollection" /> to facilitate the addition of data access
    /// services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds data access services to the service collection.
        /// </summary>
        /// <param name="services">
        /// The service collection.
        /// </param>
        /// <param name="connectionString">
        /// The database connection string.
        /// </param>
        /// <returns>
        /// The service collection to allow chaining.
        /// </returns>
        public static IServiceCollection AddDataAccessServices(
            this IServiceCollection services, string connectionString) =>
            services
                .AddTransient<IDbConnectionSource>(
                    serviceProvider => new DbConnectionSource(connectionString))
                .AddTransient<IPasswordResetTokenDataProvider, PasswordResetTokenDataProvider>()
                .AddTransient<IRecipeDataProvider, RecipeDataProvider>()
                .AddTransient<IUserDataProvider, UserDataProvider>();
    }
}
