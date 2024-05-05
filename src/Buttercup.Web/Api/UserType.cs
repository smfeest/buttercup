using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed class UserType : ObjectType<User>
{
    protected override void Configure(IObjectTypeDescriptor<User> descriptor) =>
        descriptor
            .Ignore(u => u.HashedPassword)
            .Ignore(u => u.SecurityStamp);
}
