using System;
using Microsoft.Extensions.Configuration;
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
        /// <param name="configure">
        /// An action that configures the email options.
        /// </param>
        /// <returns>
        /// The service collection to allow chaining.
        /// </returns>
        public static IServiceCollection AddEmailServices(
            this IServiceCollection services, Action<EmailOptions> configure) =>
            services.Configure(configure).AddEmailServices();

        /// <summary>
        /// Adds email services to the service collection.
        /// </summary>
        /// <param name="services">
        /// The service collection.
        /// </param>
        /// <param name="configuration">
        /// The configuration the email options should be bound against.
        /// </param>
        /// <returns>
        /// The service collection to allow chaining.
        /// </returns>
        public static IServiceCollection AddEmailServices(
            this IServiceCollection services, IConfiguration configuration) =>
            services.Configure<EmailOptions>(configuration).AddEmailServices();

        private static IServiceCollection AddEmailServices(this IServiceCollection services) =>
            services
                .AddTransient<IEmailSender, EmailSender>()
                .AddTransient<ISendGridClientAccessor, SendGridClientAccessor>();
    }
}
