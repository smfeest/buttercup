using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed class RecipeRevisionType : ObjectType<RecipeRevision>
{
    protected override void Configure(IObjectTypeDescriptor<RecipeRevision> descriptor) =>
        descriptor
            .Ignore(r => r.RecipeId)
            .Ignore(r => r.CreatedByUserId);
}
