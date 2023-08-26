using System.Security.Claims;
using System.Text.Encodings.Web;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.TestUtils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Buttercup.Web.Authentication;

public sealed class TokenAuthenticationHandlerTests : IAsyncLifetime
{
    private const string SchemeName = "ExampleScheme";

    private readonly ModelFactory modelFactory = new();

    private readonly DefaultHttpContext httpContext = new();
    private readonly Mock<ITokenAuthenticationService> tokenAuthenticationServiceMock = new();
    private readonly Mock<IUserPrincipalFactory> userPrincipalFactoryMock = new();

    private readonly TokenAuthenticationHandler tokenAuthenticationHandler;

    public TokenAuthenticationHandlerTests()
    {
        var optionsMonitor = Mock.Of<IOptionsMonitor<AuthenticationSchemeOptions>>(
            x => x.Get(SchemeName) == new AuthenticationSchemeOptions());

        this.tokenAuthenticationHandler = new(
            optionsMonitor,
            NullLoggerFactory.Instance,
            Mock.Of<UrlEncoder>(),
            Mock.Of<ISystemClock>(),
            this.tokenAuthenticationServiceMock.Object,
            this.userPrincipalFactoryMock.Object);
    }

    public Task InitializeAsync() => this.tokenAuthenticationHandler.InitializeAsync(
        new(SchemeName, null, typeof(TokenAuthenticationHandler)), this.httpContext);

    public Task DisposeAsync() => Task.CompletedTask;

    private void SetAuthorizationHeader(string value) =>
        this.httpContext.Request.Headers[HeaderNames.Authorization] = value;

    #region HandleAuthenticateAsync

    [Fact]
    public async Task HandleAuthenticateAsync_RequestHasNoAuthorizationHeader_ReturnsNoResult()
    {
        var result = await this.tokenAuthenticationHandler.AuthenticateAsync();

        Assert.True(result.None);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_AuthorizationTypeIsNotBearer_ReturnsNoResult()
    {
        this.SetAuthorizationHeader("Basic VGVzdDpUZXN0");

        var result = await this.tokenAuthenticationHandler.AuthenticateAsync();

        Assert.True(result.None);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_AccessTokenIsInvalid_ReturnsFailure()
    {
        this.SetAuthorizationHeader("Bearer invalid-token");

        this.tokenAuthenticationServiceMock
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
    public async Task HandleAuthenticateAsync_AccessTokenIsValid_ReturnsSuccess(
        string authorizationHeaderValue)
    {
        var user = this.modelFactory.BuildUser();
        var principal = new ClaimsPrincipal();

        this.SetAuthorizationHeader(authorizationHeaderValue);

        this.tokenAuthenticationServiceMock
            .Setup(x => x.ValidateAccessToken("valid-token"))
            .ReturnsAsync(user);

        this.userPrincipalFactoryMock
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
    public async Task HandleChallengeAsync_SetsStatusCodeAndWWWAuthenticateHeader()
    {
        await this.tokenAuthenticationHandler.ChallengeAsync(null);

        Assert.Equal(StatusCodes.Status401Unauthorized, this.httpContext.Response.StatusCode);
        Assert.Equal("Bearer", this.httpContext.Response.Headers[HeaderNames.WWWAuthenticate]);
    }

    #endregion
}
