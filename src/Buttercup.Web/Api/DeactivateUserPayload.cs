using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed class DeactivateUserPayload(long userId, bool deactivated)
{
    /// <summary>
    /// <b>true</b> if the user was deactivated; <b>false</b> if the user is already deactivated.
    /// </summary>
    public bool Deactivated => deactivated;

    /// <summary>
    /// The user.
    /// </summary>
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<User> User(AppDbContext dbContext) =>
        dbContext.Users.Where(u => u.Id == userId).OrderBy(u => u.Id);
}
