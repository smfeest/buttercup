using System;
using System.Collections.Generic;
using System.Security.Claims;
using Buttercup.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Buttercup.Web.Filters
{
    public class EnsureSignedOutAttributeTests
    {
        #region OnActionExecuting

        [Fact]
        public void OnActionExecutingDoesNotRedirectIfUnauthenticated()
        {
            var context = new Context();

            context.Execute();

            Assert.Null(context.ActionExecutingContext.Result);
        }

        [Fact]
        public void OnActionExecutingRedirectsToSignOutIfAuthenticated()
        {
            var context = new Context();

            context.HttpContext.Features.Set<IHttpRequestFeature>(new HttpRequestFeature
            {
                Path = "/path/to/action",
            });
            context.HttpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(Array.Empty<Claim>(), "sample-authentication-type"));

            context.Execute();

            var redirectResult = Assert.IsType<RedirectToActionResult>(
                context.ActionExecutingContext.Result);
            Assert.Equal("Authentication", redirectResult.ControllerName);
            Assert.Equal(nameof(AuthenticationController.SignOut), redirectResult.ActionName);
            Assert.Equal(
                new PathString("/path/to/action"), redirectResult.RouteValues["returnUrl"]);
        }

        [Fact]
        public void OnActionExecutingSetsCacheControlHeader()
        {
            var context = new Context();

            context.Execute();

            var cacheControlHeader = context.HttpContext.Response.GetTypedHeaders().CacheControl;

            Assert.True(cacheControlHeader.NoCache);
            Assert.True(cacheControlHeader.NoStore);
        }

        #endregion

        private class Context
        {
            public Context()
            {
                var actionContext = new ActionContext(
                    this.HttpContext, new RouteData(), new ActionDescriptor());

                this.ActionExecutingContext = new ActionExecutingContext(
                    actionContext,
                    Array.Empty<IFilterMetadata>(),
                    new Dictionary<string, object>(),
                    null);

                this.EnsureSignedOutAttribute = new EnsureSignedOutAttribute();
            }

            public EnsureSignedOutAttribute EnsureSignedOutAttribute { get; }

            public ActionExecutingContext ActionExecutingContext { get; }

            public DefaultHttpContext HttpContext { get; } = new DefaultHttpContext();

            public void Execute() =>
                this.EnsureSignedOutAttribute.OnActionExecuting(this.ActionExecutingContext);
        }
    }
}
