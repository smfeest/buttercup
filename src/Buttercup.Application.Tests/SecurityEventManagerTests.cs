using System.Net;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace Buttercup.Application;

[Collection(nameof(DatabaseCollection))]
public sealed class SecurityEventManagerTests : DatabaseTests<DatabaseCollection>
{
    private readonly ModelFactory modelFactory = new();

    private readonly FakeTimeProvider timeProvider;
    private readonly SecurityEventManager securityEventManager;

    public SecurityEventManagerTests(DatabaseFixture<DatabaseCollection> databaseFixture)
        : base(databaseFixture)
    {
        this.timeProvider = new(this.modelFactory.NextDateTime());
        this.securityEventManager = new(databaseFixture, this.timeProvider);
    }

    #region CreateSecurityEvent

    [Fact]
    public async Task CreateSecurityEvent_WithoutTime_InsertsSecurityEventWithCurrentTime()
    {
        var eventName = this.modelFactory.NextString("security-event");
        var ipAddress = new IPAddress(this.modelFactory.NextInt());
        var user = this.modelFactory.BuildUser();

        await this.DatabaseFixture.InsertEntities(user);

        var id = await this.securityEventManager.CreateSecurityEvent(eventName, ipAddress, user.Id);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        var expected = new SecurityEvent()
        {
            Id = id,
            Time = this.timeProvider.GetUtcDateTimeNow(),
            Event = eventName,
            IpAddress = ipAddress,
            UserId = user.Id,
        };
        var actual = await dbContext.SecurityEvents.FindAsync(
            [id], TestContext.Current.CancellationToken);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task CreateSecurityEvent_WithTime_InsertsSecurityEventAndReturnsId()
    {
        var time = this.modelFactory.NextDateTime();
        var eventName = this.modelFactory.NextString("security-event");
        var ipAddress = new IPAddress(this.modelFactory.NextInt());
        var user = this.modelFactory.BuildUser();

        await this.DatabaseFixture.InsertEntities(user);

        var id = await this.securityEventManager.CreateSecurityEvent(
            time, eventName, ipAddress, user.Id);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        var expected = new SecurityEvent()
        {
            Id = id,
            Time = time,
            Event = eventName,
            IpAddress = ipAddress,
            UserId = user.Id,
        };
        var actual = await dbContext.SecurityEvents.FindAsync(
            [id], TestContext.Current.CancellationToken);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task CreateSecurityEvent_AcceptsNullIpAddressAndUserId()
    {
        var eventName = this.modelFactory.NextString("security-event");

        var id = await this.securityEventManager.CreateSecurityEvent(eventName, null, null);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        var actual = await dbContext.SecurityEvents.FindAsync(
            [id], TestContext.Current.CancellationToken);

        Assert.NotNull(actual);
        Assert.Null(actual.IpAddress);
        Assert.Null(actual.UserId);
    }

    #endregion
}
