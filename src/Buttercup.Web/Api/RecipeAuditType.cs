using Buttercup.EntityModel;
using Buttercup.Web.Security;

namespace Buttercup.Web.Api;

public sealed class RecipeAuditType : ObjectType<RecipeAudit>
{
    protected override void Configure(IObjectTypeDescriptor<RecipeAudit> descriptor)
    {
        descriptor.Field(r => r.IpAddress).Authorize(AuthorizationPolicyNames.AdminOnly);

        descriptor
            .Ignore(r => r.RecipeId)
            .Ignore(r => r.RevisionId)
            .Ignore(r => r.ActorId);
    }
}
