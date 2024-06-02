using Buttercup.EntityModel;
using Buttercup.Web.Security;
using HotChocolate.Authorization;

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
            .Authorize(AuthorizationPolicyNames.SelfOrAdmin, ApplyPolicy.BeforeResolver);
        descriptor
            .Field(u => u.PasswordCreated)
            .Authorize(AuthorizationPolicyNames.SelfOrAdmin, ApplyPolicy.BeforeResolver);
        descriptor
            .Field(u => u.IsAdmin)
            .Authorize(AuthorizationPolicyNames.SelfOrAdmin, ApplyPolicy.BeforeResolver);

        descriptor
            .Ignore(u => u.HashedPassword)
            .Ignore(u => u.SecurityStamp);
    }
}
