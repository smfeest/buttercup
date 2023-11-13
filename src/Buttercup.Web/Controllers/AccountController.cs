using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Controllers;

[Authorize]
[Route("account")]
public sealed class AccountController : Controller
{
    private readonly ICookieAuthenticationService cookieAuthenticationService;
    private readonly IPasswordAuthenticationService passwordAuthenticationService;
    private readonly IStringLocalizer<AccountController> localizer;
    private readonly IUserManager userManager;

    public AccountController(
        IUserManager userManager,
        ICookieAuthenticationService cookieAuthenticationService,
        IPasswordAuthenticationService passwordAuthenticationService,
        IStringLocalizer<AccountController> localizer)
    {
        this.userManager = userManager;
        this.cookieAuthenticationService = cookieAuthenticationService;
        this.passwordAuthenticationService = passwordAuthenticationService;
        this.localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Show() => this.View(await this.GetCurrentUser());

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
                this.localizer["Error_WrongPassword"]!);

            return this.View(model);
        }

        await this.cookieAuthenticationService.RefreshPrincipal(this.HttpContext);

        return this.RedirectToAction(nameof(this.Show));
    }

    [HttpGet("preferences")]
    public async Task<IActionResult> Preferences() =>
        this.View(new PreferencesViewModel(await this.GetCurrentUser()));

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

    private Task<User> GetCurrentUser() =>
        this.userManager.GetUser(this.HttpContext.User.GetUserId());
}
