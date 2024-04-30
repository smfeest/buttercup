using System.Net;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.TestUtils;
using Buttercup.Web.Models.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Buttercup.Web.Controllers;

public sealed class AuthenticationControllerTests : IDisposable
{
    private readonly ModelFactory modelFactory = new();

    private readonly DefaultHttpContext httpContext = new();
    private readonly Mock<ICookieAuthenticationService> cookieAuthenticationServiceMock = new();
    private readonly DictionaryLocalizer<AuthenticationController> localizer = new();
    private readonly Mock<IPasswordAuthenticationService> passwordAuthenticationServiceMock = new();
    private readonly Mock<IUrlHelper> urlHelperMock = new();

    private readonly AuthenticationController authenticationController;

    public AuthenticationControllerTests() =>
        this.authenticationController = new(
            this.cookieAuthenticationServiceMock.Object,
            this.passwordAuthenticationServiceMock.Object,
            this.localizer)
        {
            ControllerContext = new()
            {
                HttpContext = this.httpContext,
            },
            Url = this.urlHelperMock.Object,
        };

    public void Dispose() => this.authenticationController.Dispose();

    #region RequestPasswordReset (GET)

    [Fact]
    public void RequestPasswordReset_Get_ReturnsViewResult()
    {
        var result = this.authenticationController.RequestPasswordReset();
        Assert.IsType<ViewResult>(result);
    }

    #endregion

    #region RequestPasswordReset (POST)

    [Fact]
    public async Task RequestPasswordReset_Post_ReturnsViewResultWhenModelIsInvalid()
    {
        this.authenticationController.ModelState.AddModelError("test", "test");

        var viewModel = new RequestPasswordResetViewModel();
        var result = await this.authenticationController.RequestPasswordReset(viewModel);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(viewModel, viewResult.Model);
    }

    [Fact]
    public async Task RequestPasswordReset_Post_SendsPasswordResetLink()
    {
        var email = this.modelFactory.NextEmail();
        var viewModel = new RequestPasswordResetViewModel { Email = email };
        var ipAddress = this.SetupRemoteIpAddress();

        var result = await this.authenticationController.RequestPasswordReset(viewModel);

        this.passwordAuthenticationServiceMock.Verify(
            x => x.SendPasswordResetLink(email, ipAddress, this.urlHelperMock.Object));
    }

    [Fact]
    public async Task RequestPasswordReset_Post_ReturnsViewResult()
    {
        var viewModel = new RequestPasswordResetViewModel();
        var result = await this.authenticationController.RequestPasswordReset(viewModel);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("RequestPasswordResetConfirmation", viewResult.ViewName);
        Assert.Same(viewModel, viewResult.Model);
    }

    #endregion

    #region ResetPassword (GET)

    [Fact]
    public async Task ResetPassword_Get_TokenIsValid_ReturnsDefaultViewResult()
    {
        var token = this.SetupResetPasswordGet(true);

        var result = await this.authenticationController.ResetPassword(token);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
    }

    [Fact]
    public async Task ResetPassword_Get_TokenIsInvalid_ReturnsInvalidTokenViewResult()
    {
        var token = this.SetupResetPasswordGet(false);

        var result = await this.authenticationController.ResetPassword(token);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("ResetPasswordInvalidToken", viewResult.ViewName);
    }

    private string SetupResetPasswordGet(bool tokenIsValue)
    {
        var token = this.modelFactory.NextString("token");
        var ipAddress = this.SetupRemoteIpAddress();

        this.passwordAuthenticationServiceMock
            .Setup(x => x.PasswordResetTokenIsValid(token, ipAddress))
            .ReturnsAsync(tokenIsValue);

        return token;
    }

    #endregion

    #region ResetPassword (POST)

    [Fact]
    public async Task ResetPassword_Post_InvalidModel_ReturnsDefaultViewResult()
    {
        this.authenticationController.ModelState.AddModelError("test", "test");

        var viewModel = new ResetPasswordViewModel();
        var result = await this.authenticationController.ResetPassword("sample-token", viewModel);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(viewModel, viewResult.Model);
    }

    [Fact]
    public async Task ResetPassword_Post_InvalidToken_ReturnsInvalidTokenViewResult()
    {
        var ipAddress = this.SetupRemoteIpAddress();

        this.passwordAuthenticationServiceMock
            .Setup(x => x.ResetPassword("sample-token", "sample-password", ipAddress))
            .ThrowsAsync(new InvalidTokenException());

        var result = await this.authenticationController.ResetPassword(
            "sample-token", new() { Password = "sample-password" });

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("ResetPasswordInvalidToken", viewResult.ViewName);
    }

    [Fact]
    public async Task ResetPassword_Post_Success_SignsInUser()
    {
        var user = this.modelFactory.BuildUser();
        var ipAddress = this.SetupRemoteIpAddress();

        this.passwordAuthenticationServiceMock
            .Setup(x => x.ResetPassword("sample-token", "sample-password", ipAddress))
            .ReturnsAsync(user);

        await this.authenticationController.ResetPassword(
            "sample-token", new() { Password = "sample-password" });

        this.cookieAuthenticationServiceMock.Verify(x => x.SignIn(this.httpContext, user));
    }

    [Fact]
    public async Task ResetPassword_Post_Success_RedirectsToHomeIndex()
    {
        var result = await this.authenticationController.ResetPassword("sample-token", new());

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Home", redirectResult.ControllerName);
        Assert.Equal(nameof(HomeController.Index), redirectResult.ActionName);
    }

    #endregion

    #region SignIn (GET)

    [Fact]
    public void SignIn_Get_ReturnsViewResult()
    {
        var result = this.authenticationController.SignIn();
        Assert.IsType<ViewResult>(result);
    }

    #endregion

    #region SignIn (POST)

    [Fact]
    public async Task SignIn_Post_InvalidModel_ReturnsViewResult()
    {
        var viewModel = this.SetupSignInPost(this.modelFactory.BuildUser());
        this.authenticationController.ModelState.AddModelError("test", "test");

        var result = await this.authenticationController.SignIn(viewModel);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(viewModel, viewResult.Model);
    }

    [Fact]
    public async Task SignIn_Post_AuthenticationFailure_AddsError()
    {
        var viewModel = this.SetupSignInPost(null);

        await this.authenticationController.SignIn(viewModel);

        var formState = this.authenticationController.ModelState[string.Empty];
        Assert.NotNull(formState);

        var error = Assert.Single(formState.Errors);
        Assert.Equal("translated-wrong-email-or-password-error", error.ErrorMessage);
    }

    [Fact]
    public async Task SignIn_Post_AuthenticationFailure_ReturnsViewResult()
    {
        var viewModel = this.SetupSignInPost(null);

        var result = await this.authenticationController.SignIn(viewModel);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(viewModel, viewResult.Model);
    }

    [Fact]
    public async Task SignIn_Post_Success_SignsInUser()
    {
        var user = this.modelFactory.BuildUser();
        var viewModel = this.SetupSignInPost(user);

        await this.authenticationController.SignIn(viewModel);

        this.cookieAuthenticationServiceMock.Verify(x => x.SignIn(this.httpContext, user));
    }

    [Fact]
    public async Task SignIn_Post_Success_RedirectsToInternalUrl()
    {
        var viewModel = this.SetupSignInPost(this.modelFactory.BuildUser());
        this.urlHelperMock.Setup(x => x.IsLocalUrl("/sample/redirect")).Returns(true);

        var result = await this.authenticationController.SignIn(viewModel, "/sample/redirect");

        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/sample/redirect", redirectResult.Url);
    }

    [Fact]
    public async Task SignIn_Post_Success_RedirectsDoesNotRedirectToExternalUrl()
    {
        var viewModel = this.SetupSignInPost(this.modelFactory.BuildUser());
        this.urlHelperMock.Setup(x => x.IsLocalUrl("https://evil.com/")).Returns(false);

        var result = await this.authenticationController.SignIn(viewModel, "https://evil.com/");

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Home", redirectResult.ControllerName);
        Assert.Equal(nameof(HomeController.Index), redirectResult.ActionName);
    }

    private SignInViewModel SetupSignInPost(User? user)
    {
        var email = this.modelFactory.NextEmail();
        var password = this.modelFactory.NextString("password");
        var ipAddress = this.SetupRemoteIpAddress();

        this.localizer.Add(
            "Error_WrongEmailOrPassword", "translated-wrong-email-or-password-error");

        this.passwordAuthenticationServiceMock
            .Setup(x => x.Authenticate(email, password, ipAddress))
            .ReturnsAsync(user);

        return new() { Email = email, Password = password };
    }

    #endregion

    #region SignOut

    [Fact]
    public async Task SignOut_SignsOutUser()
    {
        var result = await this.authenticationController.SignOut();

        this.cookieAuthenticationServiceMock.Verify(x => x.SignOut(this.httpContext));
    }

    [Fact]
    public async Task SignOut_SetsCacheControlHeader()
    {
        await this.authenticationController.SignOut();

        var cacheControlHeader = this.httpContext.Response.GetTypedHeaders().CacheControl;

        Assert.NotNull(cacheControlHeader);
        Assert.True(cacheControlHeader.NoCache);
        Assert.True(cacheControlHeader.NoStore);
    }

    [Fact]
    public async Task SignOut_RedirectsToInternalUrls()
    {
        this.urlHelperMock.Setup(x => x.IsLocalUrl("/sample/redirect")).Returns(true);

        var result = await this.authenticationController.SignOut("/sample/redirect");

        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/sample/redirect", redirectResult.Url);
    }

    [Fact]
    public async Task SignOut_DoesNotRedirectToExternalUrls()
    {
        this.urlHelperMock.Setup(x => x.IsLocalUrl("https://evil.com/")).Returns(false);

        var result = await this.authenticationController.SignOut("https://evil.com/");

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Home", redirectResult.ControllerName);
        Assert.Equal(nameof(HomeController.Index), redirectResult.ActionName);
    }

    #endregion

    private IPAddress SetupRemoteIpAddress()
    {
        var ipAddress = new IPAddress(this.modelFactory.NextInt());
        this.httpContext.Features.Set<IHttpConnectionFeature>(
            new HttpConnectionFeature { RemoteIpAddress = ipAddress });
        return ipAddress;
    }
}
