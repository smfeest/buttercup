using System.Linq.Expressions;
using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Buttercup.DataAccess;

internal sealed class UserDataProvider : IUserDataProvider
{
    private readonly IClock clock;

    public UserDataProvider(IClock clock) => this.clock = clock;

    public Task<User?> FindUserByEmail(AppDbContext dbContext, string email) =>
        dbContext.Users.SingleOrDefaultAsync(u => u.Email == email);

    public async Task<User> GetUser(AppDbContext dbContext, long id) =>
        await dbContext.Users.FindAsync(id) ?? throw UserNotFound(id);

    public Task SaveNewPassword(
        AppDbContext dbContext, long userId, string hashedPassword, string securityStamp) =>
        UpdateUserProperties(
            dbContext,
            userId,
            s => s.SetProperty(u => u.HashedPassword, hashedPassword)
                .SetProperty(u => u.SecurityStamp, securityStamp)
                .SetProperty(u => u.PasswordCreated, this.clock.UtcNow)
                .SetProperty(u => u.Modified, this.clock.UtcNow)
                .SetProperty(u => u.Revision, u => u.Revision + 1));

    public async Task<bool> SaveRehashedPassword(
        AppDbContext dbContext,
        long userId,
        int baseRevision,
        string rehashedPassword,
        DateTime timestamp)
    {
        var updatedRows = await dbContext
            .Users
            .Where(u => u.Id == userId && u.Revision == baseRevision)
            .ExecuteUpdateAsync(
                s => s.SetProperty(u => u.HashedPassword, rehashedPassword)
                    .SetProperty(u => u.Modified, timestamp)
                    .SetProperty(u => u.Revision, u => u.Revision + 1));

        return updatedRows > 0;
    }

    public Task SetTimeZone(AppDbContext dbContext, long userId, string timeZone) =>
        UpdateUserProperties(
            dbContext,
            userId,
            s => s.SetProperty(u => u.TimeZone, timeZone)
                .SetProperty(u => u.Modified, this.clock.UtcNow)
                .SetProperty(u => u.Revision, u => u.Revision + 1));

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
