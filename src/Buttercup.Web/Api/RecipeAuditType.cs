using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed class RecipeAuditType : ObjectType<RecipeAudit>
{
    protected override void Configure(IObjectTypeDescriptor<RecipeAudit> descriptor) =>
        descriptor
            .Ignore(a => a.RecipeId)
            .Ignore(a => a.RevisionId)
            .Ignore(a => a.ActorId);
}
