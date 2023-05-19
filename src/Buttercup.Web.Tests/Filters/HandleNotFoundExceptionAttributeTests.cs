using Buttercup.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Xunit;

namespace Buttercup.Web.Filters;

public class HandleNotFoundExceptionAttributeTests
{
    #region OnActionExecuting

    [Fact]
    public void OnExceptionSetsResultIfExceptionMatches()
    {
        var exceptionContext = CallOnException(new NotFoundException());

        Assert.IsType<NotFoundResult>(exceptionContext.Result);
    }

    [Fact]
    public void OnExceptionDoesNotSetResultIfExceptionDoesNotMatch()
    {
        var exceptionContext = CallOnException(new InvalidOperationException());

        Assert.Null(exceptionContext.Result);
    }

    #endregion

    private static ExceptionContext CallOnException(Exception exception)
    {
        var exceptionContext = new ExceptionContext(
            new(new DefaultHttpContext(), new(), new()), Array.Empty<IFilterMetadata>())
        {
            Exception = exception,
        };

        new HandleNotFoundExceptionAttribute().OnException(exceptionContext);

        return exceptionContext;
    }
}
