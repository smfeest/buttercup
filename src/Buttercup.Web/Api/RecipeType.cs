using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed class RecipeType : ObjectType<Recipe>
{
    protected override void Configure(IObjectTypeDescriptor<Recipe> descriptor)
    {
        descriptor.Field(u => u.Id).IsProjected(true);

        descriptor
            .Ignore(r => r.CreatedByUserId)
            .Ignore(r => r.ModifiedByUserId)
            .Ignore(r => r.DeletedByUserId);
    }
}
