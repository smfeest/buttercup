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
    private readonly IAuthenticationManager authenticationManager;
    private readonly IDbContextFactory<AppDbContext> dbContextFactory;
    private readonly IStringLocalizer<AccountController> localizer;
    private readonly IUserDataProvider userDataProvider;

    public AccountController(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IUserDataProvider userDataProvider,
        IAuthenticationManager authenticationManager,
        IStringLocalizer<AccountController> localizer)
    {
        this.dbContextFactory = dbContextFactory;
        this.userDataProvider = userDataProvider;
        this.authenticationManager = authenticationManager;
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

        var passwordChanged = await this.authenticationManager.ChangePassword(
            this.HttpContext, model.CurrentPassword, model.NewPassword);

        if (!passwordChanged)
        {
            this.ModelState.AddModelError(
                nameof(ChangePasswordViewModel.CurrentPassword),
                this.localizer["Error_WrongPassword"]!);

            return this.View(model);
        }

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

        await this.userDataProvider.UpdatePreferences(
            dbContext, this.User.GetUserId(), model.TimeZone);

        return this.RedirectToAction(nameof(this.Show));
    }

    private async Task<User> GetCurrentUser()
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return await this.userDataProvider.GetUser(dbContext, this.HttpContext.User.GetUserId());
    }
}
