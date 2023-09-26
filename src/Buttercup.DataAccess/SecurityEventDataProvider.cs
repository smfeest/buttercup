using Buttercup.EntityModel;

namespace Buttercup.DataAccess;

internal sealed class SecurityEventDataProvider : ISecurityEventDataProvider
{
    private readonly IClock clock;

    public SecurityEventDataProvider(IClock clock) => this.clock = clock;

    public async Task<long> LogEvent(
        AppDbContext dbContext, string eventName, long? userId = null, string? email = null)
    {
        var securityEvent = new SecurityEvent
        {
            Time = this.clock.UtcNow,
            Event = eventName,
            UserId = userId,
            Email = email,
        };

        dbContext.SecurityEvents.Add(securityEvent);

        await dbContext.SaveChangesAsync();

        return securityEvent.Id;
    }
}
