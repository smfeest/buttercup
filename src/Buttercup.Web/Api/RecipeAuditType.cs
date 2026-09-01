using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed class RecipeAuditType : ObjectType<RecipeAudit>
{
    protected override void Configure(IObjectTypeDescriptor<RecipeAudit> descriptor) =>
        descriptor
            .Ignore(r => r.RecipeId)
            .Ignore(r => r.RevisionId)
            .Ignore(r => r.ActorId);
}
