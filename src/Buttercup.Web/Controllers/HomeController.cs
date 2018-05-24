using Microsoft.AspNetCore.Mvc;

namespace Buttercup.Web.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("/")]
        public IActionResult Index() => this.View();
    }
}
