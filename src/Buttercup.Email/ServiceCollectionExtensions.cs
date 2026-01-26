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
    /// <returns>
    /// The service collection to allow chaining.
    /// </returns>
    public static IServiceCollection AddEmailServices(this IServiceCollection services)
    {
        services.AddOptions<EmailOptions>().BindConfiguration("Email").ValidateDataAnnotations();

        services.AddTransient<AzureEmailSender>();
        services.AddHttpClient<MailpitSender>();
        services.AddTransient<IEmailSender>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<EmailOptions>>().Value;
            return options.Provider switch
            {
                EmailProvider.Azure => provider.GetRequiredService<AzureEmailSender>(),
                EmailProvider.Mailpit => provider.GetRequiredService<MailpitSender>(),
                _ => throw new InvalidOperationException(
                    $"'{options.Provider}' is not a valid {nameof(EmailProvider)} value"),
            };
        });

        return services;
    }
}
