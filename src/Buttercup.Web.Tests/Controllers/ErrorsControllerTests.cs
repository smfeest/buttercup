using System.Diagnostics;
using Buttercup.Web.Models.Errors;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Buttercup.Web.Controllers;

public sealed class ErrorsControllerTests
{
    #region AccessDenied

    [Fact]
    public void AccessDenied_ReturnsViewResult()
    {
        using var errorsController = new ErrorsController();
        Assert.IsType<ViewResult>(errorsController.AccessDenied());
    }

    #endregion

    #region Error

    [Fact]
    public void Error_WithCurrentActivity_UsesActivityIdAsRequestId()
    {
        using var activity = new Activity("test-activity").Start();
        Activity.Current = activity;

        using var errorsController = new ErrorsController
        {
            ControllerContext = new()
            {
                HttpContext = new DefaultHttpContext { TraceIdentifier = "trace-id" }
            }
        };

        var viewResult = Assert.IsType<ViewResult>(errorsController.Error());
        var viewModel = Assert.IsType<ErrorViewModel>(viewResult.Model);
        Assert.Equal(activity.Id, viewModel.RequestId);
    }

    [Fact]
    public void Error_WithoutCurrentActivity_UsesTraceIdentifierAsRequestId()
    {
        using var errorsController = new ErrorsController
        {
            ControllerContext = new()
            {
                HttpContext = new DefaultHttpContext { TraceIdentifier = "trace-id" }
            }
        };

        var viewResult = Assert.IsType<ViewResult>(errorsController.Error());
        var viewModel = Assert.IsType<ErrorViewModel>(viewResult.Model);
        Assert.Equal("trace-id", viewModel.RequestId);
    }

    #endregion
}
