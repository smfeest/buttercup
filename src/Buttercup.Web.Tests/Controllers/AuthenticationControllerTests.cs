using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.TestUtils;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace Buttercup.Web.Controllers;

public sealed class AuthenticationControllerTests
{
    private readonly ModelFactory modelFactory = new();

    #region RequestPasswordReset (GET)

    [Fact]
    public void RequestPasswordReset_Get_ReturnsViewResult()
    {
        using var fixture = new AuthenticationControllerFixture();

        var result = fixture.AuthenticationController.RequestPasswordReset();
        Assert.IsType<ViewResult>(result);
    }

    #endregion

    #region RequestPasswordReset (POST)

    [Fact]
    public async Task RequestPasswordReset_Post_ReturnsViewResultWhenModelIsInvalid()
    {
        using var fixture = new AuthenticationControllerFixture();

        fixture.AuthenticationController.ModelState.AddModelError("test", "test");

        var model = new RequestPasswordResetViewModel();
        var result = await fixture.AuthenticationController.RequestPasswordReset(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
    }

    [Fact]
    public async Task RequestPasswordReset_Post_SendsPasswordResetLink()
    {
        using var fixture = new AuthenticationControllerFixture();

        var model = new RequestPasswordResetViewModel { Email = "sample-user@example.com" };
        var result = await fixture.AuthenticationController.RequestPasswordReset(model);

        fixture.MockPasswordAuthenticationService.Verify(x => x.SendPasswordResetLink(
            "sample-user@example.com", fixture.MockUrlHelper.Object));
    }

    [Fact]
    public async Task RequestPasswordReset_Post_ReturnsViewResult()
    {
        using var fixture = new AuthenticationControllerFixture();

        var model = new RequestPasswordResetViewModel();

        var result = await fixture.AuthenticationController.RequestPasswordReset(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("RequestPasswordResetConfirmation", viewResult.ViewName);
        Assert.Same(model, viewResult.Model);
    }

    #endregion

    #region ResetPassword (GET)

    [Fact]
    public async Task ResetPassword_Get_TokenIsValid_ReturnsDefaultViewResult()
    {
        using var fixture = new AuthenticationControllerFixture();

        fixture.MockPasswordAuthenticationService
            .Setup(x => x.PasswordResetTokenIsValid("sample-token"))
            .ReturnsAsync(true);

        var result = await fixture.AuthenticationController.ResetPassword("sample-token");

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
    }

    [Fact]
    public async Task ResetPassword_Get_TokenIsInvalid_ReturnsInvalidTokenViewResult()
    {
        using var fixture = new AuthenticationControllerFixture();

        fixture.MockPasswordAuthenticationService
            .Setup(x => x.PasswordResetTokenIsValid("sample-token"))
            .ReturnsAsync(false);

        var result = await fixture.AuthenticationController.ResetPassword("sample-token");

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("ResetPasswordInvalidToken", viewResult.ViewName);
    }

    #endregion

    #region ResetPassword (POST)

    [Fact]
    public async Task ResetPassword_Post_InvalidModel_ReturnsDefaultViewResult()
    {
        using var fixture = new AuthenticationControllerFixture();

        fixture.AuthenticationController.ModelState.AddModelError("test", "test");

        var viewModel = new ResetPasswordViewModel();
        var result = await fixture.AuthenticationController.ResetPassword(
            "sample-token", viewModel);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(viewModel, viewResult.Model);
    }

    [Fact]
    public async Task ResetPassword_Post_InvalidToken_ReturnsInvalidTokenViewResult()
    {
        using var fixture = new AuthenticationControllerFixture();

        fixture.MockPasswordAuthenticationService
            .Setup(x => x.ResetPassword("sample-token", "sample-password"))
            .ThrowsAsync(new InvalidTokenException());

        var result = await fixture.AuthenticationController.ResetPassword(
            "sample-token", new() { Password = "sample-password" });

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("ResetPasswordInvalidToken", viewResult.ViewName);
    }

    [Fact]
    public async Task ResetPassword_Post_Success_SignsInUser()
    {
        using var fixture = new AuthenticationControllerFixture();

        var user = this.modelFactory.BuildUser();

        fixture.MockPasswordAuthenticationService
            .Setup(x => x.ResetPassword("sample-token", "sample-password"))
            .ReturnsAsync(user);

        await fixture.AuthenticationController.ResetPassword(
            "sample-token", new() { Password = "sample-password" });

        fixture.MockCookieAuthenticationService.Verify(x => x.SignIn(fixture.HttpContext, user));
    }

    [Fact]
    public async Task ResetPassword_Post_Success_RedirectsToHomeIndex()
    {
        using var fixture = new AuthenticationControllerFixture();

        var result = await fixture.AuthenticationController.ResetPassword("sample-token", new());

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Home", redirectResult.ControllerName);
        Assert.Equal(nameof(HomeController.Index), redirectResult.ActionName);
    }

    #endregion

    #region SignIn (GET)

    [Fact]
    public void SignIn_Get_ReturnsViewResult()
    {
        using var fixture = new AuthenticationControllerFixture();

        var result = fixture.AuthenticationController.SignIn();
        Assert.IsType<ViewResult>(result);
    }

    #endregion

    #region SignIn (POST)

    [Fact]
    public async Task SignIn_Post_InvalidModel_ReturnsViewResult()
    {
        using var fixture = new SignInPostFixture();

        fixture.AuthenticationController.ModelState.AddModelError("test", "test");

        var result = await fixture.SignInPost();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(fixture.Model, viewResult.Model);
    }

    [Fact]
    public async Task SignIn_Post_AuthenticationFailure_AddsError()
    {
        using var fixture = new SignInPostFixture();

        fixture.SetupAuthenticate(null);

        await fixture.SignInPost();

        var formState = fixture.AuthenticationController.ModelState[string.Empty];
        Assert.NotNull(formState);

        var error = Assert.Single(formState.Errors);
        Assert.Equal("translated-wrong-email-or-password-error", error.ErrorMessage);
    }

    [Fact]
    public async Task SignIn_Post_AuthenticationFailure_ReturnsViewResult()
    {
        using var fixture = new SignInPostFixture();

        fixture.SetupAuthenticate(null);

        var result = await fixture.SignInPost();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(fixture.Model, viewResult.Model);
    }

    [Fact]
    public async Task SignIn_Post_Success_SignsInUser()
    {
        using var fixture = new SignInPostFixture();

        var user = this.modelFactory.BuildUser();

        fixture.SetupAuthenticate(user);

        await fixture.SignInPost();

        fixture.MockCookieAuthenticationService.Verify(x => x.SignIn(fixture.HttpContext, user));
    }

    [Fact]
    public async Task SignIn_Post_Success_RedirectsToInternalUrl()
    {
        using var fixture = new SignInPostFixture();

        fixture.SetupAuthenticate(this.modelFactory.BuildUser());

        fixture.MockUrlHelper.Setup(x => x.IsLocalUrl("/sample/redirect")).Returns(true);

        var result = await fixture.SignInPost("/sample/redirect");

        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/sample/redirect", redirectResult.Url);
    }

    [Fact]
    public async Task SignIn_Post_Success_RedirectsDoesNotRedirectToExternalUrl()
    {
        using var fixture = new SignInPostFixture();

        fixture.SetupAuthenticate(this.modelFactory.BuildUser());

        fixture.MockUrlHelper.Setup(x => x.IsLocalUrl("https://evil.com/")).Returns(false);

        var result = await fixture.SignInPost("https://evil.com/");

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Home", redirectResult.ControllerName);
        Assert.Equal(nameof(HomeController.Index), redirectResult.ActionName);
    }

    private sealed class SignInPostFixture : AuthenticationControllerFixture
    {
        public SignInPostFixture() =>
            this.MockLocalizer.SetupLocalizedString(
                "Error_WrongEmailOrPassword", "translated-wrong-email-or-password-error");

        public SignInViewModel Model { get; } = new()
        {
            Email = "sample@example.com",
            Password = "sample-password",
        };

        public void SetupAuthenticate(User? user) =>
            this.MockPasswordAuthenticationService
                .Setup(x => x.Authenticate("sample@example.com", "sample-password"))
                .ReturnsAsync(user);

        public Task<IActionResult> SignInPost(string? returnUrl = null) =>
            this.AuthenticationController.SignIn(this.Model, returnUrl);
    }

    #endregion

    #region SignOut

    [Fact]
    public void SignOut_SignsOutUser()
    {
        using var fixture = new AuthenticationControllerFixture();

        var result = fixture.AuthenticationController.SignOut();

        fixture.MockCookieAuthenticationService.Verify(x => x.SignOut(fixture.HttpContext));
    }

    [Fact]
    public void SignOut_SetsCacheControlHeader()
    {
        using var fixture = new AuthenticationControllerFixture();

        var result = fixture.AuthenticationController.SignOut();

        var cacheControlHeader = fixture.HttpContext.Response.GetTypedHeaders().CacheControl;

        Assert.NotNull(cacheControlHeader);
        Assert.True(cacheControlHeader.NoCache);
        Assert.True(cacheControlHeader.NoStore);
    }

    [Fact]
    public async Task SignOut_RedirectsToInternalUrls()
    {
        using var fixture = new AuthenticationControllerFixture();

        fixture.MockUrlHelper.Setup(x => x.IsLocalUrl("/sample/redirect")).Returns(true);

        var result = await fixture.AuthenticationController.SignOut("/sample/redirect");

        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/sample/redirect", redirectResult.Url);
    }

    [Fact]
    public async Task SignOut_DoesNotRedirectToExternalUrls()
    {
        using var fixture = new AuthenticationControllerFixture();

        fixture.MockUrlHelper.Setup(x => x.IsLocalUrl("https://evil.com/")).Returns(false);

        var result = await fixture.AuthenticationController.SignOut("https://evil.com/");

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Home", redirectResult.ControllerName);
        Assert.Equal(nameof(HomeController.Index), redirectResult.ActionName);
    }

    #endregion

    private class AuthenticationControllerFixture : IDisposable
    {
        public AuthenticationControllerFixture()
        {
            this.ControllerContext = new()
            {
                HttpContext = this.HttpContext,
            };

            this.AuthenticationController = new(
                this.MockCookieAuthenticationService.Object,
                this.MockPasswordAuthenticationService.Object,
                this.MockLocalizer.Object)
            {
                ControllerContext = this.ControllerContext,
                Url = this.MockUrlHelper.Object,
            };
        }

        public AuthenticationController AuthenticationController { get; }

        public ControllerContext ControllerContext { get; }

        public DefaultHttpContext HttpContext { get; } = new();

        public Mock<ICookieAuthenticationService> MockCookieAuthenticationService { get; } = new();

        public Mock<IPasswordAuthenticationService> MockPasswordAuthenticationService { get; } =
            new();

        public Mock<IStringLocalizer<AuthenticationController>> MockLocalizer { get; } = new();

        public Mock<IUrlHelper> MockUrlHelper { get; } = new();

        public void Dispose() => this.AuthenticationController?.Dispose();
    }
}
