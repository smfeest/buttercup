using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web.Areas.Admin.Controllers.Queries;

public sealed class UsersControllerQueries : IUsersControllerQueries
{
    public Task<User[]> GetUsersForIndex(AppDbContext dbContext) =>
        dbContext.Users.OrderBy(u => u.Name).ThenBy(u => u.Email).ToArrayAsync();
}
