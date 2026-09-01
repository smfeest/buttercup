using Buttercup.EntityModel;
using Buttercup.Web.Security;

namespace Buttercup.Web.Api;

public sealed class RecipeAuditType : ObjectType<RecipeAudit>
{
    protected override void Configure(IObjectTypeDescriptor<RecipeAudit> descriptor)
    {
        descriptor.Field(a => a.IpAddress).Authorize(AuthorizationPolicyNames.AdminOnly);

        descriptor
            .Ignore(a => a.RecipeId)
            .Ignore(a => a.RevisionId)
            .Ignore(a => a.ActorId);
    }
}
