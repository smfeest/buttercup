using System.Text.Encodings.Web;
using Buttercup.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Buttercup.Web.Authentication;

public sealed class TokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ITokenAuthenticationService tokenAuthenticationService;
    private readonly IUserPrincipalFactory userPrincipalFactory;

    public TokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        ITokenAuthenticationService tokenAuthenticationService,
        IUserPrincipalFactory userPrincipalFactory)
        : base(options, logger, encoder, clock)
    {
        this.tokenAuthenticationService = tokenAuthenticationService;
        this.userPrincipalFactory = userPrincipalFactory;
    }

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
