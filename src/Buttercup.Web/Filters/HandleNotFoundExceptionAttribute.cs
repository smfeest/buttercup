using System;
using Buttercup.DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Buttercup.Web.Filters
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HandleNotFoundExceptionAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is NotFoundException)
            {
                context.Result = new NotFoundResult();
            }
        }
    }
}