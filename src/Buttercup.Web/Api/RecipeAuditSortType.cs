using Buttercup.EntityModel;
using HotChocolate.Data.Sorting;

namespace Buttercup.Web.Api;

public sealed class RecipeAuditSortType : SortInputType<RecipeAudit>
{
    protected override void Configure(ISortInputTypeDescriptor<RecipeAudit> descriptor) =>
        descriptor
            .Ignore(a => a.RecipeId)
            .Ignore(a => a.RevisionId)
            .Ignore(a => a.ActorId);
}
