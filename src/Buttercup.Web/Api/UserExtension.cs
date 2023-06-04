using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

[ExtendObjectType<User>(
    IgnoreProperties = new[] { nameof(User.HashedPassword), nameof(User.SecurityStamp) })]
public static class UserExtension
{
}
