using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed class ReactivateUserPayload(long userId, bool reactivated)
{
    /// <summary>
    /// <b>true</b> if the user was reactivated; <b>false</b> if the user is already active.
    /// </summary>
    public bool Reactivated => reactivated;

    /// <summary>
    /// The user.
    /// </summary>
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<User> User(AppDbContext dbContext) =>
        dbContext.Users.Where(u => u.Id == userId).OrderBy(u => u.Id);
}
