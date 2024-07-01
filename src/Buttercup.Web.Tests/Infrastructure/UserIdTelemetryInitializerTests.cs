using System.Security.Claims;
using Microsoft.ApplicationInsights.DataContracts;
using Xunit;

namespace Buttercup.Web.Infrastructure;

public sealed class UserIdTelemetryInitializerTests
{
    private readonly HttpContextAccessor httpContextAccessor = new();

    private readonly UserIdTelemetryInitializer telemetryInitializer;

    public UserIdTelemetryInitializerTests() =>
        this.telemetryInitializer = new(this.httpContextAccessor);


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void SetsAuthenticatedUserIdWhenNullOrEmpty(string? existingUserId)
    {
        this.httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            User = new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "new-user-id")]))
        };

        var telemetry = new RequestTelemetry();
        telemetry.Context.User.AuthenticatedUserId = existingUserId;
        this.telemetryInitializer.Initialize(telemetry);

        Assert.Equal("new-user-id", telemetry.Context.User.AuthenticatedUserId);
    }

    [Fact]
    public void DoesNotSetAuthenticatedUserIdWhenAlreadySet()
    {
        this.httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            User = new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "new-user-id")]))
        };

        var telemetry = new RequestTelemetry();
        telemetry.Context.User.AuthenticatedUserId = "existing-user-id";
        this.telemetryInitializer.Initialize(telemetry);

        Assert.Equal("existing-user-id", telemetry.Context.User.AuthenticatedUserId);
    }

    [Fact]
    public void SetsAuthenticatedUserIdToNullWhenHttpContextIsNull()
    {
        this.httpContextAccessor.HttpContext = null;

        var telemetry = new RequestTelemetry();
        this.telemetryInitializer.Initialize(telemetry);

        Assert.Null(telemetry.Context.User.AuthenticatedUserId);
    }

    [Fact]
    public void SetsAuthenticatedUserIdToNullWhenPrincipalHasNoNameIdentifier()
    {
        this.httpContextAccessor.HttpContext = new DefaultHttpContext();

        var telemetry = new RequestTelemetry();
        this.telemetryInitializer.Initialize(telemetry);

        Assert.Null(telemetry.Context.User.AuthenticatedUserId);
    }
}
