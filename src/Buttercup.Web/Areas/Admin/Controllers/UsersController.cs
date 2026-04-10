using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.Web.Areas.Admin.Controllers.Queries;
using Buttercup.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Buttercup.Web.Areas.Admin.Controllers;

[Authorize(AuthorizationPolicyNames.AdminOnly)]
[Area("Admin")]
[Route("[area]/[controller]")]
public sealed class UsersController(
    IDbContextFactory<AppDbContext> dbContextFactory,
    IOptions<GlobalizationOptions> globalizationOptions,
    IStringLocalizer<UsersController> localizer,
    IUsersControllerQueries queries,
    IUserManager userManager) : Controller
{
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;
    private readonly GlobalizationOptions globalizationOptions = globalizationOptions.Value;
    private readonly IStringLocalizer<UsersController> localizer = localizer;
    private readonly IUsersControllerQueries queries = queries;
    private readonly IUserManager userManager = userManager;

    [HttpGet]
    public async Task<ViewResult> Index(CancellationToken cancellationToken)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();
        var users = await this.queries.GetUsersForIndex(dbContext, cancellationToken);
        return this.View(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Show(long id, CancellationToken cancellationToken)
    {
        var user = await this.userManager.FindUser(id, cancellationToken);
        return user is null ? this.NotFound() : this.View(user);
    }

    [HttpGet("new")]
    public IActionResult New() =>
        this.View(
            new NewUserAttributes { TimeZone = this.globalizationOptions.DefaultUserTimeZone });

    [HttpPost("new")]
    public async Task<IActionResult> New(
        NewUserAttributes model, CancellationToken cancellationToken)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        try
        {
            await this.userManager.CreateUser(
                model,
                this.User.GetUserId(),
                this.HttpContext.Connection.RemoteIpAddress,
                cancellationToken);
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
    public async Task<IActionResult> Deactivate(long id, CancellationToken cancellationToken)
    {
        try
        {
            await this.userManager.DeactivateUser(
                id,
                this.User.GetUserId(),
                this.HttpContext.Connection.RemoteIpAddress,
                cancellationToken);

            return this.RedirectToAction(nameof(this.Show), new { id });

        }
        catch (NotFoundException)
        {
            return this.NotFound();
        }
    }

    [HttpPost("{id}/reactivate")]
    public async Task<IActionResult> Reactivate(long id, CancellationToken cancellationToken)
    {
        try
        {
            await this.userManager.ReactivateUser(
                id,
                this.User.GetUserId(),
                this.HttpContext.Connection.RemoteIpAddress,
                cancellationToken);

            return this.RedirectToAction(nameof(this.Show), new { id });

        }
        catch (NotFoundException)
        {
            return this.NotFound();
        }
    }
}
