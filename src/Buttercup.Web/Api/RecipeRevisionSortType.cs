using Buttercup.EntityModel;
using HotChocolate.Data.Sorting;

namespace Buttercup.Web.Api;

public sealed class RecipeRevisionSortType : SortInputType<RecipeRevision>
{
    protected override void Configure(ISortInputTypeDescriptor<RecipeRevision> descriptor) =>
        descriptor
            .Ignore(r => r.Recipe)
            .Ignore(r => r.RecipeId);
}
