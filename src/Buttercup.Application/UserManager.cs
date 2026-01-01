using System.Linq.Expressions;
using System.Net;
using Buttercup.EntityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Buttercup.Application;

internal sealed class UserManager(
    IDbContextFactory<AppDbContext> dbContextFactory,
    IPasswordHasher<User> passwordHasher,
    IRandomTokenGenerator randomTokenGenerator,
    TimeProvider timeProvider)
    : IUserManager
{
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;
    private readonly IPasswordHasher<User> passwordHasher = passwordHasher;
    private readonly IRandomTokenGenerator randomTokenGenerator = randomTokenGenerator;
    private readonly TimeProvider timeProvider = timeProvider;

    public async Task<long> CreateUser(
        NewUserAttributes attributes, long currentUserId, IPAddress? ipAddress)
    {
        var timestamp = this.timeProvider.GetUtcDateTimeNow();
        var user = new User
        {
            Name = attributes.Name,
            Email = attributes.Email,
            SecurityStamp = this.GenerateSecurityStamp(),
            TimeZone = attributes.TimeZone,
            IsAdmin = attributes.IsAdmin,
            Created = timestamp,
            Modified = timestamp,
        };

        using var dbContext = this.dbContextFactory.CreateDbContext();
        dbContext.Users.Add(user);
        dbContext.UserAuditEntries.Add(
            new()
            {
                Time = timestamp,
                Operation = UserAuditOperation.Create,
                Target = user,
                ActorId = currentUserId,
                IpAddress = ipAddress
            });

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

    public async Task<(long Id, string Password)> CreateTestUser()
    {
        var suffix = this.randomTokenGenerator.Generate(2);
        var password = this.randomTokenGenerator.Generate(4);
        var timestamp = this.timeProvider.GetUtcDateTimeNow();

        var user = new User
        {
            Name = $"Test User {suffix}",
            Email = $"test+{suffix}@example.com",
            PasswordCreated = timestamp,
            SecurityStamp = this.GenerateSecurityStamp(),
            TimeZone = "Etc/UTC",
            IsAdmin = false,
            Created = timestamp,
            Modified = timestamp,
        };

        user.HashedPassword = this.passwordHasher.HashPassword(user, password);

        using var dbContext = this.dbContextFactory.CreateDbContext();
        dbContext.Users.Add(user);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException) when (dbContext.Users.Any(u => u.Email == user.Email))
        {
            return await this.CreateTestUser();
        }

        return (user.Id, password);
    }

    public async Task<bool> DeactivateUser(long id, long currentUserId, IPAddress? ipAddress)
    {
        var timestamp = this.timeProvider.GetUtcDateTimeNow();

        using var dbContext = this.dbContextFactory.CreateDbContext();

        var user = await dbContext.Users.AsTracking().GetAsync(id);

        if (user.Deactivated.HasValue)
        {
            return false;
        }

        user.SecurityStamp = this.GenerateSecurityStamp();
        user.Modified = timestamp;
        user.Deactivated = timestamp;
        user.Revision++;

        dbContext.UserAuditEntries.Add(
            new()
            {
                Time = timestamp,
                Operation = UserAuditOperation.Deactivate,
                TargetId = id,
                ActorId = currentUserId,
                IpAddress = ipAddress
            });

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return await this.DeactivateUser(id, currentUserId, ipAddress);
        }

        return true;
    }

    public async Task<User?> FindUser(long id)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return await dbContext.Users.FindAsync(id);
    }

    public async Task<bool> HardDeleteTestUser(long id)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        await dbContext.SecurityEvents.Where(e => e.UserId == id).ExecuteDeleteAsync();

        return await dbContext.Users.Where(u => u.Id == id).ExecuteDeleteAsync() != 0;
    }

    public async Task<bool> ReactivateUser(long id, long currentUserId, IPAddress? ipAddress)
    {
        var timestamp = this.timeProvider.GetUtcDateTimeNow();

        using var dbContext = this.dbContextFactory.CreateDbContext();

        var user = await dbContext.Users.AsTracking().GetAsync(id);

        if (!user.Deactivated.HasValue)
        {
            return false;
        }

        user.Modified = timestamp;
        user.Deactivated = null;
        user.Revision++;

        dbContext.UserAuditEntries.Add(
            new()
            {
                Time = timestamp,
                Operation = UserAuditOperation.Reactivate,
                TargetId = id,
                ActorId = currentUserId,
                IpAddress = ipAddress
            });

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return await this.ReactivateUser(id, currentUserId, ipAddress);
        }

        return true;
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

    private string GenerateSecurityStamp() => this.randomTokenGenerator.Generate(2);

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
