using System.Net;
using Buttercup.EntityModel;

namespace Buttercup.DataAccess;

internal sealed class SecurityEventDataProvider : ISecurityEventDataProvider
{
    private readonly IClock clock;

    public SecurityEventDataProvider(IClock clock) => this.clock = clock;

    public async Task<long> LogEvent(
        AppDbContext dbContext, string eventName, IPAddress? ipAddress, long? userId = null)
    {
        var securityEvent = new SecurityEvent
        {
            Time = this.clock.UtcNow,
            Event = eventName,
            IpAddress = ipAddress,
            UserId = userId,
        };

        dbContext.SecurityEvents.Add(securityEvent);

        await dbContext.SaveChangesAsync();

        return securityEvent.Id;
    }
}
