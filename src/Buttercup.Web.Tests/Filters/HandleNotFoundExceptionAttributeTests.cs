using Buttercup.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Xunit;

namespace Buttercup.Web.Filters;

public sealed class HandleNotFoundExceptionAttributeTests
{
    #region OnException

    [Fact]
    public void OnException_ExceptionMatches_SetsResult()
    {
        var exceptionContext = CallOnException(new NotFoundException());

        Assert.IsType<NotFoundResult>(exceptionContext.Result);
    }

    [Fact]
    public void OnException_ExceptionDoesNotMatch_DoesNotSetResult()
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
