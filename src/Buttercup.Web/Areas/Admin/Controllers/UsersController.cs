using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.Web.Areas.Admin.Controllers.Queries;
using Buttercup.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web.Areas.Admin.Controllers;

[Authorize(AuthorizationPolicyNames.AdminOnly)]
[Area("Admin")]
[Route("[area]/[controller]")]
public sealed class UsersController(
    IDbContextFactory<AppDbContext> dbContextFactory,
    IUsersControllerQueries queries,
    IUserManager userManager) : Controller
{
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;
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
}
