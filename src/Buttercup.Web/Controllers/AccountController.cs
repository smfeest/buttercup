using System.Threading.Tasks;
using Buttercup.Web.Authentication;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Controllers
{
    [Authorize]
    [Route("account")]
    public class AccountController : Controller
    {
        public AccountController(
            IAuthenticationManager authenticationManager,
            IStringLocalizer<AccountController> localizer)
        {
            this.AuthenticationManager = authenticationManager;
            this.Localizer = localizer;
        }

        public IAuthenticationManager AuthenticationManager { get; }

        public IStringLocalizer<AccountController> Localizer { get; }

        [HttpGet]
        public IActionResult Show() => this.View(this.HttpContext.GetCurrentUser());

        [HttpGet("change-password")]
        public IActionResult ChangePassword() => this.View();

        [HttpPost("change-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var passwordChanged = await this.AuthenticationManager.ChangePassword(
                this.HttpContext, model.CurrentPassword, model.NewPassword);

            if (!passwordChanged)
            {
                this.ModelState.AddModelError(
                    nameof(ChangePasswordViewModel.CurrentPassword),
                    this.Localizer["Error_WrongPassword"]);

                return this.View(model);
            }

            return this.RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
