using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed class CreateTestUserPayload(long userId, string password)
{
    /// <summary>
    /// The user.
    /// </summary>
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<User> User(AppDbContext dbContext) =>
        dbContext.Users.Where(r => r.Id == userId).OrderBy(r => r.Id);

    /// <summary>
    /// The user's initial password.
    /// </summary>
    public string Password { get; } = password;
}
