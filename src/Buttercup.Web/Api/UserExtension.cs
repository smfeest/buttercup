using Buttercup.Models;

namespace Buttercup.Web.Api;

[ExtendObjectType(
    typeof(User),
    IgnoreProperties = new[] { nameof(User.HashedPassword), nameof(User.SecurityStamp) })]
public class UserExtension
{
}
