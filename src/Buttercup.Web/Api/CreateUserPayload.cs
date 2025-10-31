using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed class CreateUserPayload(long userId)
{
    /// <summary>
    /// The user.
    /// </summary>
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<User> User(AppDbContext dbContext) =>
        dbContext.Users.Where(r => r.Id == userId).OrderBy(r => r.Id);
}
