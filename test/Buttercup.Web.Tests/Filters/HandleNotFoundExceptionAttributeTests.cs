using System;
using Buttercup.DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Buttercup.Web.Filters
{
    public class HandleNotFoundExceptionAttributeTests
    {
        #region OnActionExecuting

        [Fact]
        public void OnExceptionSetsResultIfExceptionMatches()
        {
            var context = new Context(new NotFoundException());

            context.Execute();

            Assert.IsType<NotFoundResult>(context.ExceptionContext.Result);
        }

        [Fact]
        public void OnExceptionDoesNotSetResultIfExceptionDoesNotMatch()
        {
            var context = new Context(new Exception());

            context.Execute();

            Assert.Null(context.ExceptionContext.Result);
        }

        #endregion

        private class Context
        {
            public Context(Exception exception)
            {
                this.ExceptionContext = new(
                    new(new DefaultHttpContext(), new(), new()),
                    Array.Empty<IFilterMetadata>())
                {
                    Exception = exception,
                };
            }

            public HandleNotFoundExceptionAttribute HandleNotFoundExceptionAttribute { get; } = new();

            public ExceptionContext ExceptionContext { get; }

            public void Execute() =>
                this.HandleNotFoundExceptionAttribute.OnException(this.ExceptionContext);
        }
    }
}
