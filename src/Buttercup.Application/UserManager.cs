using System.Linq.Expressions;
using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Buttercup.Application;

internal sealed class UserManager(IClock clock, IDbContextFactory<AppDbContext> dbContextFactory)
    : IUserManager
{
    private readonly IClock clock = clock;
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;

    public async Task<User> GetUser(long id)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return await dbContext.Users.FindAsync(id) ?? throw UserNotFound(id);
    }

    public async Task SetTimeZone(long userId, string timeZone)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        await UpdateUserProperties(
            dbContext,
            userId,
            s => s.SetProperty(u => u.TimeZone, timeZone)
                .SetProperty(u => u.Modified, this.clock.UtcNow)
                .SetProperty(u => u.Revision, u => u.Revision + 1));
    }

    private static async Task UpdateUserProperties(
        AppDbContext dbContext,
        long userId,
        Expression<Func<SetPropertyCalls<User>, SetPropertyCalls<User>>> setPropertyCalls)
    {
        var updatedRows = await dbContext
            .Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(setPropertyCalls);

        if (updatedRows == 0)
        {
            throw UserNotFound(userId);
        }
    }

    private static NotFoundException UserNotFound(long userId) => new($"User {userId} not found");
}
