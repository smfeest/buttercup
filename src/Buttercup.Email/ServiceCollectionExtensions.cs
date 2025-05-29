using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Buttercup.Email;

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
        services.AddEmailServices(options => options.Configure(configure));

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
        services.AddEmailServices(options => options.Bind(configuration));

    private static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        Action<OptionsBuilder<EmailOptions>> buildOptionsAction)
    {
        buildOptionsAction(services.AddOptions<EmailOptions>().ValidateDataAnnotations());

        return services.AddTransient<IEmailSender, EmailSender>();
    }
}
