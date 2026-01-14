using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.Web.Areas.Admin.Controllers.Queries;
using Buttercup.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Areas.Admin.Controllers;

[Authorize(AuthorizationPolicyNames.AdminOnly)]
[Area("Admin")]
[Route("[area]/[controller]")]
public sealed class UsersController(
    IDbContextFactory<AppDbContext> dbContextFactory,
    IStringLocalizer<UsersController> localizer,
    IUsersControllerQueries queries,
    IUserManager userManager) : Controller
{
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;
    private readonly IStringLocalizer<UsersController> localizer = localizer;
    private readonly IUsersControllerQueries queries = queries;
    private readonly IUserManager userManager = userManager;

    [HttpGet]
    public async Task<ViewResult> Index()
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();
        var users = await this.queries.GetUsersForIndex(dbContext);
        return this.View(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Show(long id)
    {
        var user = await this.userManager.FindUser(id);
        return user is null ? this.NotFound() : this.View(user);
    }

    [HttpGet("new")]
    public IActionResult New() =>
        this.View(new NewUserAttributes { TimeZone = "Europe/London" });

    [HttpPost("new")]
    public async Task<IActionResult> New(NewUserAttributes model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        try
        {
            await this.userManager.CreateUser(
                model, this.User.GetUserId(), this.HttpContext.Connection.RemoteIpAddress);
        }
        catch (NotUniqueException ex) when (ex.PropertyName == nameof(NewUserAttributes.Email))
        {
            this.ModelState.AddModelError(
                nameof(NewUserAttributes.Email),
                this.localizer["Error_EmailNotUnique"]);

            return this.View(model);
        }

        return this.RedirectToAction(nameof(this.Index));
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(long id)
    {
        try
        {
            await this.userManager.DeactivateUser(
                id, this.User.GetUserId(), this.HttpContext.Connection.RemoteIpAddress);

            return this.RedirectToAction(nameof(this.Show), new { id });

        }
        catch (NotFoundException)
        {
            return this.NotFound();
        }
    }

    [HttpPost("{id}/reactivate")]
    public async Task<IActionResult> Reactivate(long id)
    {
        try
        {
            await this.userManager.ReactivateUser(
                id, this.User.GetUserId(), this.HttpContext.Connection.RemoteIpAddress);

            return this.RedirectToAction(nameof(this.Show), new { id });

        }
        catch (NotFoundException)
        {
            return this.NotFound();
        }
    }
}
