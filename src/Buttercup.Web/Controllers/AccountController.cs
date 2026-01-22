using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.Web.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Controllers;

[Authorize]
[Route("account")]
public sealed class AccountController(
    IUserManager userManager,
    ICookieAuthenticationService cookieAuthenticationService,
    IPasswordAuthenticationService passwordAuthenticationService,
    IStringLocalizer<AccountController> localizer)
    : Controller
{
    private readonly ICookieAuthenticationService cookieAuthenticationService =
        cookieAuthenticationService;
    private readonly IPasswordAuthenticationService passwordAuthenticationService =
        passwordAuthenticationService;
    private readonly IStringLocalizer<AccountController> localizer = localizer;
    private readonly IUserManager userManager = userManager;

    [HttpGet]
    public async Task<IActionResult> Show()
    {
        var user = await this.FindCurrentUser();
        return user is null ? this.NotFound() : this.View(user);
    }

    [HttpGet("change-password")]
    public IActionResult ChangePassword() => this.View();

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        var userId = this.HttpContext.User.GetUserId();
        var ipAddress = this.HttpContext.Connection.RemoteIpAddress;

        var passwordChanged = await this.passwordAuthenticationService.ChangePassword(
            userId, model.CurrentPassword, model.NewPassword, ipAddress);

        if (!passwordChanged)
        {
            this.ModelState.AddModelError(
                nameof(ChangePasswordViewModel.CurrentPassword),
                this.localizer["Error_WrongPassword"]);

            return this.View(model);
        }

        await this.cookieAuthenticationService.RefreshPrincipal(this.HttpContext);

        return this.RedirectToAction(nameof(this.Show));
    }

    [HttpGet("preferences")]
    public async Task<IActionResult> Preferences()
    {
        var user = await this.FindCurrentUser();
        return user is null ? this.NotFound() : this.View(new PreferencesViewModel(user));
    }

    [HttpPost("preferences")]
    public async Task<IActionResult> Preferences(PreferencesViewModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        await this.userManager.SetTimeZone(this.User.GetUserId(), model.TimeZone);

        return this.RedirectToAction(nameof(this.Show));
    }

    private Task<User?> FindCurrentUser() =>
        this.userManager.FindUser(this.HttpContext.User.GetUserId());
}
