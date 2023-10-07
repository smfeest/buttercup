using System.Net;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.DataAccess;

[Collection(nameof(DatabaseCollection))]
public sealed class SecurityEventDataProviderTests
{
    private readonly DatabaseCollectionFixture databaseFixture;
    private readonly ModelFactory modelFactory = new();

    private readonly StoppedClock clock = new();
    private readonly SecurityEventDataProvider securityEventDataProvider;

    public SecurityEventDataProviderTests(DatabaseCollectionFixture databaseFixture)
    {
        this.databaseFixture = databaseFixture;
        this.securityEventDataProvider = new(this.clock);
        this.clock.UtcNow = this.modelFactory.NextDateTime();
    }

    #region LogEvent

    [Fact]
    public async Task LogEvent_InsertsEvent()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var user = this.modelFactory.BuildUser();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var eventName = this.modelFactory.NextString("event");
        var ipAddress = new IPAddress(this.modelFactory.NextInt());

        var id = await this.securityEventDataProvider.LogEvent(
            dbContext, eventName, ipAddress, user.Id);

        var expectedEvent = new SecurityEvent
        {
            Id = id,
            Time = this.clock.UtcNow,
            Event = eventName,
            IpAddress = ipAddress,
            UserId = user.Id
        };

        dbContext.ChangeTracker.Clear();

        var actualEvent = await dbContext.SecurityEvents.FindAsync(id);

        Assert.Equal(expectedEvent, actualEvent);
    }

    [Fact]
    public async Task LogEvent_AcceptsNullIpAddressAndUserId()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var eventName = this.modelFactory.NextString("event");

        var id = await this.securityEventDataProvider.LogEvent(dbContext, eventName, null);

        var expectedEvent = new SecurityEvent
        {
            Id = id,
            Time = this.clock.UtcNow,
            Event = eventName,
            IpAddress = null,
            UserId = null
        };

        dbContext.ChangeTracker.Clear();

        var actualEvent = await dbContext.SecurityEvents.FindAsync(id);

        Assert.Equal(expectedEvent, actualEvent);
    }

    #endregion
}
