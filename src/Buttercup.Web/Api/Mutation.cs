using Buttercup.Security;

namespace Buttercup.Web.Api;

[MutationType]
public sealed class Mutation
{
    public async Task<AuthenticatePayload> Authenticate(
        [Service] IPasswordAuthenticationService passwordAuthenticationService,
        [Service] ITokenAuthenticationService tokenAuthenticationService,
        AuthenticateInput input)
    {
        var user = await passwordAuthenticationService.Authenticate(input.Email, input.Password);

        if (user == null)
        {
            return new(false);
        }

        var accessToken = await tokenAuthenticationService.IssueAccessToken(user);

        return new(true, accessToken, user);
    }
}
