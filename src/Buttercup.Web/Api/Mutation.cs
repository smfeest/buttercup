using Buttercup.Web.Authentication;

namespace Buttercup.Web.Api;

public sealed class Mutation
{
    public async Task<AuthenticatePayload> Authenticate(
        [Service] IAuthenticationManager authenticationManager,
        [Service] ITokenAuthenticationService tokenAuthenticationService,
        AuthenticateInput input)
    {
        var user = await authenticationManager.Authenticate(input.Email, input.Password);

        if (user == null)
        {
            return new(false);
        }

        var accessToken = await tokenAuthenticationService.IssueAccessToken(user);

        return new(true, accessToken, user);
    }
}
