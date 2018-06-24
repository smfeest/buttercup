using System.Threading.Tasks;
using Buttercup.Web.Authentication;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Buttercup.Web.Controllers
{
    public class AuthenticationController : Controller
    {
        public AuthenticationController(IAuthenticationManager authenticationManager)
        {
            this.AuthenticationManager = authenticationManager;
        }

        public IAuthenticationManager AuthenticationManager { get; }

        [HttpGet("reset-password")]
        public IActionResult RequestPasswordReset() => this.View();

        [HttpPost("reset-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestPasswordReset(RequestPasswordResetViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            await this.AuthenticationManager.SendPasswordResetLink(model.Email);

            return this.View("RequestPasswordResetConfirmation", model);
        }

        [HttpGet("sign-in")]
        public async Task<IActionResult> SignIn()
        {
            await this.AuthenticationManager.SignOut(this.HttpContext);

            return this.View();
        }

        [HttpPost("sign-in")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInViewModel model, string returnUrl = null)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this.AuthenticationManager.Authenticate(model.Email, model.Password);

            if (user == null)
            {
                this.ModelState.AddModelError(string.Empty, "Wrong email address or password");

                return this.View(model);
            }

            await this.AuthenticationManager.SignIn(this.HttpContext, user);

            if (this.Url.IsLocalUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }
            else
            {
                return this.RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}
