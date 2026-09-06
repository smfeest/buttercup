using Buttercup.EntityModel;
using HotChocolate.Data.Filters;

namespace Buttercup.Web.Api;

public sealed class RecipeRevisionFilterType : FilterInputType<RecipeRevision>
{
    protected override void Configure(IFilterInputTypeDescriptor<RecipeRevision> descriptor) =>
        descriptor
            .Ignore(r => r.Recipe)
            .Ignore(r => r.RecipeId);
}
