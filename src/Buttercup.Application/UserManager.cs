using System.Linq.Expressions;
using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Buttercup.Application;

internal sealed class UserManager(
    IDbContextFactory<AppDbContext> dbContextFactory,
    IRandomTokenGenerator randomTokenGenerator,
    TimeProvider timeProvider)
    : IUserManager
{
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;
    private readonly IRandomTokenGenerator randomTokenGenerator = randomTokenGenerator;
    private readonly TimeProvider timeProvider = timeProvider;

    public async Task<long> CreateUser(NewUserAttributes attributes)
    {
        var timestamp = this.timeProvider.GetUtcDateTimeNow();
        var user = new User
        {
            Name = attributes.Name,
            Email = attributes.Email,
            SecurityStamp = this.randomTokenGenerator.Generate(2),
            TimeZone = attributes.TimeZone,
            IsAdmin = attributes.IsAdmin,
            Created = timestamp,
            Modified = timestamp,
        };

        using var dbContext = this.dbContextFactory.CreateDbContext();
        dbContext.Users.Add(user);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (dbContext.Users.Any(u => u.Email == attributes.Email))
        {
            throw new NotUniqueException(
                nameof(attributes.Email),
                $"Another user already exists with email '{attributes.Email}'",
                ex);
        }

        return user.Id;
    }

    public async Task<User?> FindUser(long id)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return await dbContext.Users.FindAsync(id);
    }

    public async Task SetTimeZone(long userId, string timeZone)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        await UpdateUserProperties(
            dbContext,
            userId,
            s => s.SetProperty(u => u.TimeZone, timeZone)
                .SetProperty(u => u.Modified, this.timeProvider.GetUtcDateTimeNow())
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
