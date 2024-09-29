using Buttercup.EntityModel;
using Buttercup.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web.Areas.Admin.Controllers;

[Authorize(AuthorizationPolicyNames.AdminOnly)]
[Area("Admin")]
[Route("[area]/[controller]")]
public sealed class UsersController(IDbContextFactory<AppDbContext> dbContextFactory) : Controller
{
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;

    [HttpGet]
    public async Task<ViewResult> Index()
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();
        var users = await dbContext.Users.OrderBy(u => u.Name).ThenBy(u => u.Email).ToArrayAsync();
        return this.View(users);
    }
}
