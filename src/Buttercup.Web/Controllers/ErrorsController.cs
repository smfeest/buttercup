using System.Diagnostics;
using Buttercup.Web.Models.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Buttercup.Web.Controllers;

public sealed class ErrorsController : Controller
{
    [HttpGet("/access-denied")]
    public IActionResult AccessDenied() => this.View();

    [Route("/error")]
    public IActionResult Error() => this.View(new ErrorViewModel(
        Activity.Current?.Id ?? this.HttpContext.TraceIdentifier));
}
