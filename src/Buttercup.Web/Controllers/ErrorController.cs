using System.Diagnostics;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Buttercup.Web.Controllers;

public sealed class ErrorController : Controller
{
    [Route("/error")]
    public IActionResult Error() => this.View(new ErrorViewModel(
        Activity.Current?.Id ?? this.HttpContext.TraceIdentifier));
}
