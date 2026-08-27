using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Buttercup.TestUtils;

/// <summary>
/// Provides extension methods for <see cref="HttpContext" />.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Sets <see cref="ConnectionInfo.RemoteIpAddress"/> on the <see
    /// cref="HttpContext.Connection"/> for an <see cref="HttpContext"/>.
    /// </summary>
    /// <param name="httpContext">
    /// The HTTP context.
    /// </param>
    /// <param name="ipAddress">
    /// The IP address.
    /// </param>
    public static void SetRemoteIpAddress(this HttpContext httpContext, IPAddress ipAddress) =>
        httpContext.Features.Set<IHttpConnectionFeature>(
            new HttpConnectionFeature { RemoteIpAddress = ipAddress });
}
