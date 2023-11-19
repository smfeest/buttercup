using Buttercup.Security;
using Buttercup.Web.Filters;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Controllers;

public sealed class AuthenticationController(
    ICookieAuthenticationService cookieAuthenticationService,
    IPasswordAuthenticationService passwordAuthenticationService,
    IStringLocalizer<AuthenticationController> localizer)
    : Controller
{
    private readonly ICookieAuthenticationService cookieAuthenticationService =
        cookieAuthenticationService;
    private readonly IPasswordAuthenticationService passwordAuthenticationService =
        passwordAuthenticationService;
    private readonly IStringLocalizer<AuthenticationController> localizer = localizer;

    [HttpGet("reset-password")]
    public IActionResult RequestPasswordReset() => this.View();

    [HttpPost("reset-password")]
    public async Task<IActionResult> RequestPasswordReset(RequestPasswordResetViewModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        await this.passwordAuthenticationService.SendPasswordResetLink(
            model.Email, this.HttpContext.Connection.RemoteIpAddress, this.Url);

        return this.View("RequestPasswordResetConfirmation", model);
    }

    [HttpGet("reset-password/{token}", Name = "ResetPassword")]
    [EnsureSignedOut]
    public async Task<IActionResult> ResetPassword(string token) =>
        await this.passwordAuthenticationService.PasswordResetTokenIsValid(
            token, this.HttpContext.Connection.RemoteIpAddress) ?
            this.View() :
            this.View("ResetPasswordInvalidToken");

    [HttpPost("reset-password/{token}")]
    public async Task<IActionResult> ResetPassword(string token, ResetPasswordViewModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        try
        {
            var user = await this.passwordAuthenticationService.ResetPassword(
                token, model.Password, this.HttpContext.Connection.RemoteIpAddress);

            await this.cookieAuthenticationService.SignIn(this.HttpContext, user);

            return this.RedirectToHome();
        }
        catch (InvalidTokenException)
        {
            return this.View("ResetPasswordInvalidToken");
        }
    }

    [HttpGet("sign-in")]
    [EnsureSignedOut]
    public IActionResult SignIn() => this.View();

    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl = null)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        var user = await this.passwordAuthenticationService.Authenticate(
            model.Email, model.Password, this.HttpContext.Connection.RemoteIpAddress);

        if (user == null)
        {
            this.ModelState.AddModelError(
                string.Empty, this.localizer["Error_WrongEmailOrPassword"]!);

            return this.View(model);
        }

        await this.cookieAuthenticationService.SignIn(this.HttpContext, user);

        return this.Url.IsLocalUrl(returnUrl) ?
            this.Redirect(returnUrl) :
            this.RedirectToHome();
    }

    [HttpGet("sign-out")]
    public async Task<IActionResult> SignOut(string? returnUrl = null)
    {
        await this.cookieAuthenticationService.SignOut(this.HttpContext);

        this.HttpContext.Response.GetTypedHeaders().CacheControl =
            new() { NoCache = true, NoStore = true };

        return this.Url.IsLocalUrl(returnUrl) ?
            this.Redirect(returnUrl) :
            this.RedirectToHome();
    }

    private RedirectToActionResult RedirectToHome() =>
        this.RedirectToAction(nameof(HomeController.Index), "Home");
}
