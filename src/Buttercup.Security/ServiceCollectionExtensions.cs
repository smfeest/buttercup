using Buttercup.EntityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

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
    /// <returns>
    /// The service collection to allow chaining.
    /// </returns>
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        services
            .AddOptions<SecurityOptions>()
            .BindConfiguration("Security")
            .ValidateDataAnnotations();

        return services
            .AddTransient<IPasswordHasher<User>, PasswordHasher<User>>()
            .AddTransient<IAccessTokenEncoder, AccessTokenEncoder>()
            .AddTransient<IAccessTokenSerializer, AccessTokenSerializer>()
            .AddTransient<IAuthenticationMailer, AuthenticationMailer>()
            .AddTransient<ICookieAuthenticationService, CookieAuthenticationService>()
            .AddTransient<IClaimsIdentityFactory, ClaimsIdentityFactory>()
            .AddTransient<IParameterMaskingService, ParameterMaskingService>()
            .AddTransient<IPasswordAuthenticationRateLimiter, PasswordAuthenticationRateLimiter>()
            .AddTransient<IPasswordAuthenticationService, PasswordAuthenticationService>()
            .AddTransient<IPasswordResetRateLimiter, PasswordResetRateLimiter>()
            .AddTransient<ITokenAuthenticationService, TokenAuthenticationService>();
    }
}
