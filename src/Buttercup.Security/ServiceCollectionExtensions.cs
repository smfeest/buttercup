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
    public static IServiceCollection AddSecurityServices(this IServiceCollection services) =>
        services
            .AddTransient<IPasswordHasher<User>, PasswordHasher<User>>()
            .AddTransient<IAccessTokenEncoder, AccessTokenEncoder>()
            .AddTransient<IAccessTokenSerializer, AccessTokenSerializer>()
            .AddTransient<IAuthenticationMailer, AuthenticationMailer>()
            .AddTransient<IAuthenticationManager, AuthenticationManager>()
            .AddTransient<IRandomNumberGeneratorFactory, RandomNumberGeneratorFactory>()
            .AddTransient<IRandomTokenGenerator, RandomTokenGenerator>()
            .AddTransient<ITokenAuthenticationService, TokenAuthenticationService>()
            .AddTransient<IUserPrincipalFactory, UserPrincipalFactory>();
}
