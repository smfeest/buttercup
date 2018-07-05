using System;
using Buttercup.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Buttercup.Web.Filters
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class EnsureSignedOutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult(
                    nameof(AuthenticationController.SignOut),
                    "Authentication",
                    new { returnUrl = context.HttpContext.Request.Path });
            }
        }
    }
}
