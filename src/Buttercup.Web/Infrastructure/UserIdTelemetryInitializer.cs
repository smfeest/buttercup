using System.Security.Claims;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Buttercup.Web.Infrastructure;

public sealed class UserIdTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
    : ITelemetryInitializer
{
    private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;

    public void Initialize(ITelemetry telemetry)
    {
        if (string.IsNullOrEmpty(telemetry.Context.User.AuthenticatedUserId))
        {
            telemetry.Context.User.AuthenticatedUserId =
                this.httpContextAccessor.HttpContext?.User
                    .FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
