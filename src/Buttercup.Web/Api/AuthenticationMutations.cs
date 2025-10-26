using System.Security.Claims;
using Buttercup.Security;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Api;

[MutationType]
public sealed class AuthenticationMutations
{
    public async Task<AuthenticatePayload> Authenticate(
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<AuthenticationMutations> localizer,
        IPasswordAuthenticationService passwordAuthenticationService,
        ITokenAuthenticationService tokenAuthenticationService,
        IClaimsIdentityFactory claimsIdentityFactory,
        ClaimsPrincipal claimsPrincipal,
        string email,
        string password)
    {
        var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;

        var result = await passwordAuthenticationService.Authenticate(email, password, ipAddress);

        if (!result.IsSuccess)
        {
            AuthenticateError error =
                result.Failure == PasswordAuthenticationFailure.TooManyAttempts ?
                new TooManyAttemptsError(localizer["Error_TooManyAuthenticationAttempts"]) :
                new IncorrectCredentialsError(localizer["Error_WrongEmailOrPassword"]);

            return new(error);
        }

        var accessToken = await tokenAuthenticationService.IssueAccessToken(result.User, ipAddress);

        claimsPrincipal.AddIdentity(claimsIdentityFactory.CreateIdentityForUser(result.User));

        return new(accessToken, result.User);
    }
}
