using Buttercup.EntityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Buttercup.Security;

/// <summary>
/// Extends <see cref="IServiceCollection" /> to facilitate the addition of security-related
/// services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds security-related services to the service collection.
    /// </summary>
    /// <param name="services">
    /// The service collection.
    /// </param>
    /// <param name="configure">
    /// An action that configures the security options.
    /// </param>
    /// <returns>
    /// The service collection to allow chaining.
    /// </returns>
    public static IServiceCollection AddSecurityServices(
        this IServiceCollection services, Action<SecurityOptions> configure) =>
        services.AddSecurityServices(options => options.Configure(configure));

    /// <summary>
    /// Adds security-related services to the service collection.
    /// </summary>
    /// <param name="services">
    /// The service collection.
    /// </param>
    /// <param name="configuration">
    /// The configuration the security options should be bound against.
    /// </param>
    /// <returns>
    /// The service collection to allow chaining.
    /// </returns>
    public static IServiceCollection AddSecurityServices(
        this IServiceCollection services, IConfiguration configuration) =>
        services.AddSecurityServices(options => options.Bind(configuration));


    private static IServiceCollection AddSecurityServices(
        this IServiceCollection services,
        Action<OptionsBuilder<SecurityOptions>> buildOptionsAction)
    {
        buildOptionsAction(services.AddOptions<SecurityOptions>().ValidateDataAnnotations());

        return services
            .AddTransient<IPasswordHasher<User>, PasswordHasher<User>>()
            .AddTransient<IAccessTokenEncoder, AccessTokenEncoder>()
            .AddTransient<IAccessTokenSerializer, AccessTokenSerializer>()
            .AddTransient<IAuthenticationMailer, AuthenticationMailer>()
            .AddTransient<ICookieAuthenticationService, CookieAuthenticationService>()
            .AddTransient<IClaimsIdentityFactory, ClaimsIdentityFactory>()
            .AddTransient<IPasswordAuthenticationRateLimiter, PasswordAuthenticationRateLimiter>()
            .AddTransient<IPasswordAuthenticationService, PasswordAuthenticationService>()
            .AddTransient<IRandomNumberGeneratorFactory, RandomNumberGeneratorFactory>()
            .AddTransient<IRandomTokenGenerator, RandomTokenGenerator>()
            .AddTransient<ITokenAuthenticationService, TokenAuthenticationService>();
    }
}
