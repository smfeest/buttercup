using System.Security.Claims;
using System.Text.Encodings.Web;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Buttercup.Web.Authentication;

public class TokenAuthenticationHandlerTests : IAsyncLifetime
{
    private const string SchemeName = "ExampleScheme";

    private readonly TokenAuthenticationHandler tokenAuthenticationHandler;
    private readonly DefaultHttpContext httpContext = new();
    private readonly Mock<ITokenAuthenticationService> mockTokenAuthenticationService = new();
    private readonly Mock<IUserPrincipalFactory> mockUserPrincipalFactory = new();
    private readonly ModelFactory modelFactory = new();

    public TokenAuthenticationHandlerTests()
    {
        var optionsMonitor = Mock.Of<IOptionsMonitor<AuthenticationSchemeOptions>>(
            x => x.Get(SchemeName) == new AuthenticationSchemeOptions());

        this.tokenAuthenticationHandler = new(
            optionsMonitor,
            NullLoggerFactory.Instance,
            Mock.Of<UrlEncoder>(),
            Mock.Of<ISystemClock>(),
            this.mockTokenAuthenticationService.Object,
            this.mockUserPrincipalFactory.Object);
    }

    public Task InitializeAsync() => this.tokenAuthenticationHandler.InitializeAsync(
        new(SchemeName, null, typeof(TokenAuthenticationHandler)), this.httpContext);

    public Task DisposeAsync() => Task.CompletedTask;

    private void SetAuthorizationHeader(string value) =>
        this.httpContext.Request.Headers[HeaderNames.Authorization] = value;

    #region HandleAuthenticateAsync

    [Fact]
    public async Task HandleAuthenticateAsyncReturnsNoResultWhenRequestHasNoAuthorizationHeader()
    {
        var result = await this.tokenAuthenticationHandler.AuthenticateAsync();

        Assert.True(result.None);
    }

    [Fact]
    public async Task HandleAuthenticateAsyncReturnsNoResultWhenAuthorizationTypeIsNotBearer()
    {
        this.SetAuthorizationHeader("Basic VGVzdDpUZXN0");

        var result = await this.tokenAuthenticationHandler.AuthenticateAsync();

        Assert.True(result.None);
    }

    [Fact]
    public async Task HandleAuthenticateAsyncReturnsFailureWhenAccessTokenIsInvalid()
    {
        this.SetAuthorizationHeader("Bearer invalid-token");

        this.mockTokenAuthenticationService
            .Setup(x => x.ValidateAccessToken("invalid-token"))
            .ReturnsAsync(default(User?));

        var result = await this.tokenAuthenticationHandler.AuthenticateAsync();

        Assert.NotNull(result.Failure);
        Assert.Equal("Invalid access token", result.Failure.Message);
    }

    [Theory]
    [InlineData("Bearer valid-token")]
    [InlineData("bEARER valid-token")]
    [InlineData("Bearer  valid-token ")]
    public async Task HandleAuthenticateAsyncReturnsSuccessWhenAccessTokenIsValid(
        string authorizationHeaderValue)
    {
        var user = this.modelFactory.BuildUser();
        var principal = new ClaimsPrincipal();

        this.SetAuthorizationHeader(authorizationHeaderValue);

        this.mockTokenAuthenticationService
            .Setup(x => x.ValidateAccessToken("valid-token"))
            .ReturnsAsync(user);

        this.mockUserPrincipalFactory
            .Setup(x => x.Create(user, SchemeName))
            .Returns(principal);

        var result = await this.tokenAuthenticationHandler.AuthenticateAsync();

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Ticket);
        Assert.Equal(SchemeName, result.Ticket.AuthenticationScheme);
        Assert.Same(principal, result.Ticket.Principal);
    }

    #endregion

    #region HandleChallengeAsync

    [Fact]
    public async Task HandleChallengeAsyncSetsStatusCodeAndWWWAuthenticateHeader()
    {
        await this.tokenAuthenticationHandler.ChallengeAsync(null);

        Assert.Equal(StatusCodes.Status401Unauthorized, this.httpContext.Response.StatusCode);
        Assert.Equal("Bearer", this.httpContext.Response.Headers[HeaderNames.WWWAuthenticate]);
    }

    #endregion
}
