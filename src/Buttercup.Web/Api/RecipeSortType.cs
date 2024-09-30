using Buttercup.EntityModel;
using HotChocolate.Data.Sorting;

namespace Buttercup.Web.Api;

public sealed class RecipeSortType : SortInputType<Recipe>
{
    protected override void Configure(ISortInputTypeDescriptor<Recipe> descriptor) =>
        descriptor
            .Ignore(r => r.CreatedByUserId)
            .Ignore(r => r.ModifiedByUserId)
            .Ignore(r => r.DeletedByUserId)
            .Ignore(r => r.Revision);
}
