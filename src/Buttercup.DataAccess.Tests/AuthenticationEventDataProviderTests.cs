using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.DataAccess;

[Collection(nameof(DatabaseCollection))]
public sealed class AuthenticationEventDataProviderTests
{
    private readonly DatabaseCollectionFixture databaseFixture;
    private readonly ModelFactory modelFactory = new();

    private readonly StoppedClock clock = new();
    private readonly AuthenticationEventDataProvider authenticationEventDataProvider;

    public AuthenticationEventDataProviderTests(DatabaseCollectionFixture databaseFixture)
    {
        this.databaseFixture = databaseFixture;
        this.authenticationEventDataProvider = new(this.clock);
        this.clock.UtcNow = this.modelFactory.NextDateTime();
    }

    #region LogEvent

    [Fact]
    public async Task LogEventInsertsEvent()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var user = this.modelFactory.BuildUser();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var eventName = this.modelFactory.NextString("event");
        var email = this.modelFactory.NextEmail();

        var id = await this.authenticationEventDataProvider.LogEvent(
            dbContext, eventName, user.Id, email);

        var expectedEvent = new AuthenticationEvent
        {
            Id = id,
            Time = this.clock.UtcNow,
            Event = eventName,
            UserId = user.Id,
            Email = email
        };

        dbContext.ChangeTracker.Clear();

        var actualEvent = await dbContext.AuthenticationEvents.FindAsync(id);

        Assert.Equal(expectedEvent, actualEvent);
    }

    [Fact]
    public async Task LogEventAcceptsNullUserIdAndEmail()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var eventName = this.modelFactory.NextString("event");

        var id = await this.authenticationEventDataProvider.LogEvent(dbContext, eventName);

        var expectedEvent = new AuthenticationEvent
        {
            Id = id,
            Time = this.clock.UtcNow,
            Event = eventName,
            UserId = null,
            Email = null
        };

        dbContext.ChangeTracker.Clear();

        var actualEvent = await dbContext.AuthenticationEvents.FindAsync(id);

        Assert.Equal(expectedEvent, actualEvent);
    }

    #endregion
}
