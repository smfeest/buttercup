using System;
using System.Collections.Generic;
using System.Security.Claims;
using Buttercup.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Xunit;

namespace Buttercup.Web.Filters
{
    public class EnsureSignedOutAttributeTests
    {
        private const string RequestPath = "/path/to/action";

        #region OnActionExecuting

        [Fact]
        public void OnActionExecutingDoesNotRedirectIfUnauthenticated()
        {
            var actionExecutingContext = CallOnActionExecuting(null);

            Assert.Null(actionExecutingContext.Result);
        }

        [Fact]
        public void OnActionExecutingRedirectsToSignOutIfAuthenticated()
        {
            var actionExecutingContext = CallOnActionExecuting("authentication-type");

            var redirectResult = Assert.IsType<RedirectToActionResult>(
                actionExecutingContext.Result);

            Assert.Equal("Authentication", redirectResult.ControllerName);
            Assert.Equal(nameof(AuthenticationController.SignOut), redirectResult.ActionName);
            Assert.Equal(new PathString(RequestPath), redirectResult.RouteValues["returnUrl"]);
        }

        [Fact]
        public void OnActionExecutingSetsCacheControlHeader()
        {
            var actionExecutingContext = CallOnActionExecuting(null);

            var cacheControlHeader =
                actionExecutingContext.HttpContext.Response.GetTypedHeaders().CacheControl;

            Assert.True(cacheControlHeader.NoCache);
            Assert.True(cacheControlHeader.NoStore);
        }

        #endregion

        private static ActionExecutingContext CallOnActionExecuting(string? authenticationType)
        {
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(Array.Empty<Claim>(), authenticationType));

            var httpContext = new DefaultHttpContext { User = user };

            httpContext.Features.Set<IHttpRequestFeature>(
                new HttpRequestFeature { Path = RequestPath });

            var actionExecutingContext = new ActionExecutingContext(
                new(httpContext, new(), new()),
                Array.Empty<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new());

            new EnsureSignedOutAttribute().OnActionExecuting(actionExecutingContext);

            return actionExecutingContext;
        }
    }
}
