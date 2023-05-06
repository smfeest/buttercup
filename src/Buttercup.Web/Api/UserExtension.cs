using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web.Api;

[ExtendObjectType<User>(
    IgnoreProperties = new[] { nameof(User.HashedPassword), nameof(User.SecurityStamp) })]
public class UserExtension
{
    [DataLoader]
    public static async Task<IReadOnlyDictionary<long, User>> GetUsersByIdAsync(
        IReadOnlyList<long> keys,
        IDbContextFactory<AppDbContext> dbContextFactory,
        IUserDataProvider userDataProvider)
    {
        using var dbContext = dbContextFactory.CreateDbContext();

        var users = await userDataProvider.GetUsers(dbContext, keys);

        return users.ToDictionary(x => x.Id);
    }
}
