using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.DataAccess;

internal sealed class PasswordResetTokenDataProvider : IPasswordResetTokenDataProvider
{
    private readonly IClock clock;

    public PasswordResetTokenDataProvider(IClock clock) => this.clock = clock;

    public Task DeleteExpiredTokens(AppDbContext dbContext, DateTime cutOff) =>
        dbContext.PasswordResetTokens.Where(t => t.Created < cutOff).ExecuteDeleteAsync();

    public Task DeleteTokensForUser(AppDbContext dbContext, long userId) =>
        dbContext.PasswordResetTokens.Where(t => t.UserId == userId).ExecuteDeleteAsync();

    public Task<long?> GetUserIdForToken(AppDbContext dbContext, string token) =>
        dbContext
            .PasswordResetTokens.Where(t => t.Token == token)
            .Select<PasswordResetToken, long?>(t => t.UserId)
            .SingleOrDefaultAsync();

    public async Task InsertToken(AppDbContext dbContext, long userId, string token)
    {
        dbContext.PasswordResetTokens.Add(new()
        {
            Token = token,
            UserId = userId,
            Created = this.clock.UtcNow,
        });

        await dbContext.SaveChangesAsync();
    }
}
