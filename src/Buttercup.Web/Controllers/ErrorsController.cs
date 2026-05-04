using System.Diagnostics;
using Buttercup.Web.Models.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Buttercup.Web.Controllers;

public sealed class ErrorsController : Controller
{
    [Route("/error/{statusCode}")]
    public IActionResult Error(int statusCode) =>
        this.View(
            statusCode switch
            {
                StatusCodes.Status403Forbidden => "AccessDenied",
                StatusCodes.Status404NotFound => "NotFound",
                _ => "Error"
            },
            new ErrorViewModel(Activity.Current?.Id ?? this.HttpContext.TraceIdentifier));
}
