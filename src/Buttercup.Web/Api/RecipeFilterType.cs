using Buttercup.EntityModel;
using HotChocolate.Data.Filters;

namespace Buttercup.Web.Api;

public sealed class RecipeFilterType : FilterInputType<Recipe>
{
    protected override void Configure(IFilterInputTypeDescriptor<Recipe> descriptor) =>
        descriptor
            .Ignore(r => r.CreatedByUserId)
            .Ignore(r => r.ModifiedByUserId)
            .Ignore(r => r.DeletedByUserId)
            .Ignore(r => r.Revision)
            .Ignore(r => r.Revisions);
}
