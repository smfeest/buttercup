using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Controllers;

[Authorize]
[Route("account")]
public sealed class AccountController : Controller
{
    private readonly ICookieAuthenticationService cookieAuthenticationService;
    private readonly IDbContextFactory<AppDbContext> dbContextFactory;
    private readonly IPasswordAuthenticationService passwordAuthenticationService;
    private readonly IStringLocalizer<AccountController> localizer;
    private readonly IUserDataProvider userDataProvider;

    public AccountController(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IUserDataProvider userDataProvider,
        ICookieAuthenticationService cookieAuthenticationService,
        IPasswordAuthenticationService passwordAuthenticationService,
        IStringLocalizer<AccountController> localizer)
    {
        this.dbContextFactory = dbContextFactory;
        this.userDataProvider = userDataProvider;
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

        using var dbContext = this.dbContextFactory.CreateDbContext();

        await this.userDataProvider.SetTimeZone(
            dbContext, this.User.GetUserId(), model.TimeZone);

        return this.RedirectToAction(nameof(this.Show));
    }

    private async Task<User> GetCurrentUser()
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return await this.userDataProvider.GetUser(dbContext, this.HttpContext.User.GetUserId());
    }
}
