using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Moq;
using Xunit;

namespace Buttercup.DataAccess;

[Collection("Database collection")]
public class AuthenticationEventDataProviderTests
{
    private readonly DateTime fakeTime = new(2020, 1, 2, 3, 4, 5);
    private readonly AuthenticationEventDataProvider authenticationEventDataProvider;

    public AuthenticationEventDataProviderTests()
    {
        var clock = Mock.Of<IClock>(x => x.UtcNow == this.fakeTime);

        this.authenticationEventDataProvider = new(clock);
    }

    #region LogEvent

    [Fact]
    public async Task LogEventInsertsEvent()
    {
        using var dbContext = TestDatabase.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var user = new ModelFactory().BuildUser();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var id = await this.authenticationEventDataProvider.LogEvent(
            dbContext, "sample-event", user.Id, "sample@example.com");

        var expectedEvent = new AuthenticationEvent
        {
            Id = id,
            Time = this.fakeTime,
            Event = "sample-event",
            UserId = user.Id,
            Email = "sample@example.com"
        };

        dbContext.ChangeTracker.Clear();

        var actualEvent = await dbContext.AuthenticationEvents.FindAsync(id);

        Assert.Equal(expectedEvent, actualEvent);
    }

    [Fact]
    public async Task LogEventAcceptsNullUserIdAndEmail()
    {
        using var dbContext = TestDatabase.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var id = await this.authenticationEventDataProvider.LogEvent(dbContext, "sample-event");

        var expectedEvent = new AuthenticationEvent
        {
            Id = id,
            Time = this.fakeTime,
            Event = "sample-event",
            UserId = null,
            Email = null
        };

        dbContext.ChangeTracker.Clear();

        var actualEvent = await dbContext.AuthenticationEvents.FindAsync(id);

        Assert.Equal(expectedEvent, actualEvent);
    }

    #endregion
}
