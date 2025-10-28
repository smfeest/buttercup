using System.Net;
using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Application;

internal sealed class SecurityEventManager(
    IDbContextFactory<AppDbContext> dbContextFactory, TimeProvider timeProvider)
    : ISecurityEventManager
{
    private readonly TimeProvider timeProvider = timeProvider;
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;

    public Task<long> CreateSecurityEvent(
        string eventName, IPAddress? ipAddress, long? userId) =>
        this.CreateSecurityEvent(
            this.timeProvider.GetUtcDateTimeNow(), eventName, ipAddress, userId);

    public async Task<long> CreateSecurityEvent(
        DateTime time, string eventName, IPAddress? ipAddress, long? userId)
    {
        var securityEvent = new SecurityEvent()
        {
            Time = time,
            Event = eventName,
            IpAddress = ipAddress,
            UserId = userId,
        };

        using var dbContext = this.dbContextFactory.CreateDbContext();
        dbContext.SecurityEvents.Add(securityEvent);
        await dbContext.SaveChangesAsync();

        return securityEvent.Id;
    }
}
