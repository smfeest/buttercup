using Buttercup.DataAccess;
using Buttercup.Web.Authentication;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Controllers;

[Authorize]
[Route("account")]
public class AccountController : Controller
{
    private readonly IAuthenticationManager authenticationManager;
    private readonly IClock clock;
    private readonly IMySqlConnectionSource mySqlConnectionSource;
    private readonly IStringLocalizer<AccountController> localizer;
    private readonly IUserDataProvider userDataProvider;

    public AccountController(
        IClock clock,
        IMySqlConnectionSource mySqlConnectionSource,
        IUserDataProvider userDataProvider,
        IAuthenticationManager authenticationManager,
        IStringLocalizer<AccountController> localizer)
    {
        this.clock = clock;
        this.mySqlConnectionSource = mySqlConnectionSource;
        this.userDataProvider = userDataProvider;
        this.authenticationManager = authenticationManager;
        this.localizer = localizer;
    }

    [HttpGet]
    public IActionResult Show() => this.View(this.HttpContext.GetCurrentUser());

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
            this.HttpContext, model.CurrentPassword!, model.NewPassword!);

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
    public IActionResult Preferences() =>
        this.View(new PreferencesViewModel(this.HttpContext.GetCurrentUser()!));

    [HttpPost("preferences")]
    public async Task<IActionResult> Preferences(PreferencesViewModel model)
    {
        using var connection = await this.mySqlConnectionSource.OpenConnection();

        var user = this.HttpContext.GetCurrentUser()!;

        await this.userDataProvider.UpdatePreferences(
            connection, user.Id, model.TimeZone!, this.clock.UtcNow);

        return this.RedirectToAction(nameof(this.Show));
    }
}
