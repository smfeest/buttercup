using Microsoft.Extensions.DependencyInjection;

namespace Buttercup.Email
{
    /// <summary>
    /// Extends <see cref="IServiceCollection" /> to facilitate the addition of email services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds email services to the service collection.
        /// </summary>
        /// <param name="services">
        /// The service collection.
        /// </param>
        /// <returns>
        /// The service collection to allow chaining.
        /// </returns>
        public static IServiceCollection AddEmailServices(this IServiceCollection services) =>
            services
                .AddTransient<IEmailSender, EmailSender>()
                .AddTransient<ISendGridClientAccessor, SendGridClientAccessor>();
    }
}
