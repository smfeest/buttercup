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
    IUsersControllerQueries queries) : Controller
{
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;
    private readonly IUsersControllerQueries queries = queries;

    [HttpGet]
    public async Task<ViewResult> Index()
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();
        var users = await this.queries.GetUsersForIndex(dbContext);
        return this.View(users);
    }
}
