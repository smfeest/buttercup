using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.DataAccess;

internal sealed class PasswordResetTokenDataProvider : IPasswordResetTokenDataProvider
{
    private readonly IClock clock;

    public PasswordResetTokenDataProvider(IClock clock) => this.clock = clock;

    public Task DeleteTokensForUser(AppDbContext dbContext, long userId) =>
        dbContext.PasswordResetTokens.Where(t => t.UserId == userId).ExecuteDeleteAsync();

    public Task<long?> GetUserIdForUnexpiredToken(
        AppDbContext dbContext, string token, TimeSpan maxAge) =>
        dbContext.PasswordResetTokens
            .Where(t => t.Token == token && t.Created >= this.clock.UtcNow.Subtract(maxAge))
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
