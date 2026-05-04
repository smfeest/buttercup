using System.Diagnostics;
using Buttercup.Web.Models.Errors;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Buttercup.Web.Controllers;

public sealed class ErrorsControllerTests : IDisposable
{
    private readonly ErrorsController errorsController;

    public ErrorsControllerTests() =>
        this.errorsController = new()
        {
            ControllerContext = new()
            {
                HttpContext = new DefaultHttpContext { TraceIdentifier = "trace-id" }
            }
        };

    public void Dispose() => this.errorsController.Dispose();

    #region Error

    [Fact]
    public void Error_WithCurrentActivity_UsesActivityIdAsRequestId()
    {
        using var activity = new Activity("test-activity").Start();
        Activity.Current = activity;

        var viewResult = Assert.IsType<ViewResult>(this.errorsController.Error(500));
        var viewModel = Assert.IsType<ErrorViewModel>(viewResult.Model);
        Assert.Equal(activity.Id, viewModel.RequestId);
    }

    [Fact]
    public void Error_WithoutCurrentActivity_UsesTraceIdentifierAsRequestId()
    {
        var viewResult = Assert.IsType<ViewResult>(this.errorsController.Error(500));
        var viewModel = Assert.IsType<ErrorViewModel>(viewResult.Model);
        Assert.Equal("trace-id", viewModel.RequestId);
    }

    [Theory]
    [InlineData(StatusCodes.Status400BadRequest, "Error")]
    [InlineData(StatusCodes.Status403Forbidden, "AccessDenied")]
    [InlineData(StatusCodes.Status404NotFound, "NotFound")]
    [InlineData(StatusCodes.Status500InternalServerError, "Error")]
    public void Error_RendersViewForStatusCode(int statusCode, string expectedViewName)
    {
        var viewResult = Assert.IsType<ViewResult>(this.errorsController.Error(statusCode));
        Assert.Equal(expectedViewName, viewResult.ViewName);
    }

    #endregion
}
