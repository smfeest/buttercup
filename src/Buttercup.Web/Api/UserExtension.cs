using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

[ExtendObjectType<User>(
    IgnoreProperties = [nameof(User.HashedPassword), nameof(User.SecurityStamp)])]
public static class UserExtension
{
}
