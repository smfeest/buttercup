using System.Text.Encodings.Web;
using Buttercup.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Buttercup.Web.Security;

public sealed class TokenAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ITokenAuthenticationService tokenAuthenticationService,
    IUserPrincipalFactory userPrincipalFactory)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly ITokenAuthenticationService tokenAuthenticationService =
        tokenAuthenticationService;
    private readonly IUserPrincipalFactory userPrincipalFactory = userPrincipalFactory;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = this.ReadTokenFromHeaders();

        if (string.IsNullOrEmpty(token))
        {
            return AuthenticateResult.NoResult();
        }

        var user = await this.tokenAuthenticationService.ValidateAccessToken(token);

        if (user == null)
        {
            return AuthenticateResult.Fail("Invalid access token");
        }

        var principal = this.userPrincipalFactory.Create(user, this.Scheme.Name);

        var ticket = new AuthenticationTicket(principal, this.Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        await base.HandleChallengeAsync(properties);

        this.Response.Headers.Append(HeaderNames.WWWAuthenticate, "Bearer");
    }

    private string? ReadTokenFromHeaders()
    {
        var headerValue = this.Request.Headers[HeaderNames.Authorization].ToString();

        return headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ?
            headerValue[7..].Trim() : null;
    }
}
