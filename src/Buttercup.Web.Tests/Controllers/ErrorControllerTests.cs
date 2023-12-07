using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Buttercup.Web.Controllers;

public sealed class ErrorControllerTests
{
    #region AccessDenied

    [Fact]
    public void AccessDenied_ReturnsViewResult()
    {
        using var errorController = new ErrorController();
        Assert.IsType<ViewResult>(errorController.AccessDenied());
    }

    #endregion
}
