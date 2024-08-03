using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed record IdName(long Id, string Name)
{
    public static IdName? From(User? user) => user is null ? null : new(user.Id, user.Name);
}
