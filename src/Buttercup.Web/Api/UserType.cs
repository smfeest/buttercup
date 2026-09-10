using Buttercup.EntityModel;
using Buttercup.Web.Security;

namespace Buttercup.Web.Api;

public sealed class UserType : ObjectType<User>
{
    protected override void Configure(IObjectTypeDescriptor<User> descriptor)
    {
        descriptor
            .Field(u => u.Id)
            .IsProjected(true);
        descriptor
            .Field(u => u.Email)
            .Authorize(AuthorizationPolicyNames.ParentResultSelfOrAdmin);
        descriptor
            .Field(u => u.PasswordCreated)
            .Authorize(AuthorizationPolicyNames.ParentResultSelfOrAdmin);
        descriptor
            .Field(u => u.IsAdmin)
            .Authorize(AuthorizationPolicyNames.ParentResultSelfOrAdmin);

        descriptor
            .Ignore(u => u.HashedPassword)
            .Ignore(u => u.SecurityStamp);
    }
}
