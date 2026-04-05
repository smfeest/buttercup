using Buttercup.EntityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web;

internal sealed class DevelopmentDatabaseSeeder(
    IPasswordHasher<User> passwordHasher,
    IRandomTokenGenerator randomTokenGenerator,
    TimeProvider timeProvider) : IDatabaseSeeder
{
    private readonly IPasswordHasher<User> passwordHasher = passwordHasher;
    private readonly IRandomTokenGenerator randomTokenGenerator = randomTokenGenerator;
    private readonly TimeProvider timeProvider = timeProvider;

    public void SeedDatabase(DbContext dbContext)
    {
        if (!dbContext.Set<User>().Any())
        {
            this.AddUsers(dbContext);

            dbContext.Database.AutoSavepointsEnabled = false;
            dbContext.SaveChanges();
        }
    }

    public async Task SeedDatabaseAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        if (!await dbContext.Set<User>().AnyAsync(cancellationToken))
        {
            this.AddUsers(dbContext);

            dbContext.Database.AutoSavepointsEnabled = false;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private void AddUsers(DbContext dbContext) =>
        dbContext.Set<User>().AddRange(
            this.BuildUser("Developer", "dev", Role.Admin),
            this.BuildUser("E2E Admin", "e2e-admin", Role.Admin),
            this.BuildUser("E2E User", "e2e-user", Role.Contributor));

    private User BuildUser(string name, string username, Role role)
    {
        var timestamp = this.timeProvider.GetUtcDateTimeNow();

        var user = new User
        {
            Name = name,
            Email = $"{username}@example.com",
            PasswordCreated = timestamp,
            SecurityStamp = this.randomTokenGenerator.Generate(2),
            TimeZone = "Etc/UTC",
            Role = role,
            Created = timestamp,
            Modified = timestamp,
        };

        user.HashedPassword = this.passwordHasher.HashPassword(user, $"{username}-pass");

        return user;
    }
}
