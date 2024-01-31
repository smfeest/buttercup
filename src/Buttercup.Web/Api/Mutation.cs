using Buttercup.Security;

namespace Buttercup.Web.Api;

[MutationType]
public sealed class Mutation
{
    public async Task<AuthenticatePayload> Authenticate(
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] IPasswordAuthenticationService passwordAuthenticationService,
        [Service] ITokenAuthenticationService tokenAuthenticationService,
        string email,
        string password)
    {
        var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;

        var user = await passwordAuthenticationService.Authenticate(email, password, ipAddress);

        if (user == null)
        {
            return new(false);
        }

        var accessToken = await tokenAuthenticationService.IssueAccessToken(user, ipAddress);

        return new(true, accessToken, user);
    }
}
