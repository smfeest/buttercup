using Buttercup.EntityModel;

namespace Buttercup.DataAccess;

internal sealed class AuthenticationEventDataProvider : IAuthenticationEventDataProvider
{
    private readonly IClock clock;

    public AuthenticationEventDataProvider(IClock clock) => this.clock = clock;

    public async Task<long> LogEvent(
        AppDbContext dbContext, string eventName, long? userId = null, string? email = null)
    {
        var authenticationEvent = new AuthenticationEvent
        {
            Time = this.clock.UtcNow,
            Event = eventName,
            UserId = userId,
            Email = email,
        };

        dbContext.AuthenticationEvents.Add(authenticationEvent);

        await dbContext.SaveChangesAsync();

        return authenticationEvent.Id;
    }
}
