using Buttercup.EntityModel;
using Buttercup.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web;

internal sealed partial class E2eDatabaseInitializer(
    IDbContextFactory<AppDbContext> dbContextFactory,
    ILogger<E2eDatabaseInitializer> logger,
    IPasswordHasher<User> passwordHasher,
    IRandomTokenGenerator randomTokenGenerator,
    TimeProvider timeProvider)
{
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;
    private readonly ILogger<E2eDatabaseInitializer> logger = logger;
    private readonly IPasswordHasher<User> passwordHasher = passwordHasher;
    private readonly IRandomTokenGenerator randomTokenGenerator = randomTokenGenerator;
    private readonly TimeProvider timeProvider = timeProvider;

    /// <summary>
    /// Creates and seeds the database, if it doesn't already exist.
    /// </summary>
    /// <returns>A task for the operation.</returns>
    public async Task EnsureInitialized()
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        if (!await dbContext.Database.EnsureCreatedAsync())
        {
            this.LogDatabaseAlreadyExists();
            return;
        }

        dbContext.Users.AddRange(
            this.BuildUser("E2E Admin", "e2e-admin", true),
            this.BuildUser("E2E User", "e2e-user", false));

        await dbContext.SaveChangesAsync();

        this.LogDatabaseSuccessfullyInitialized();
    }

    private User BuildUser(string name, string username, bool isAdmin)
    {
        var timestamp = this.timeProvider.GetUtcDateTimeNow();

        var user = new User
        {
            Name = name,
            Email = $"{username}@example.com",
            PasswordCreated = timestamp,
            SecurityStamp = this.randomTokenGenerator.Generate(2),
            TimeZone = "Etc/UTC",
            IsAdmin = isAdmin,
            Created = timestamp,
            Modified = timestamp,
        };

        user.HashedPassword = this.passwordHasher.HashPassword(user, $"{username}-pass");

        return user;
    }

    [LoggerMessage(
        EventId = 400,
        EventName = "DatabaseAlreadyExists",
        Level = LogLevel.Information,
        Message = "End-to-end test database already exists")]
    private partial void LogDatabaseAlreadyExists();

    [LoggerMessage(
        EventId = 401,
        EventName = "DatabaseSuccessfullyInitialized",
        Level = LogLevel.Information,
        Message = "End-to-end test database successfully initialized")]
    private partial void LogDatabaseSuccessfullyInitialized();
}
