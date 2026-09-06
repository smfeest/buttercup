using Buttercup.EntityModel;
using HotChocolate.Data.Filters;

namespace Buttercup.Web.Api;

public sealed class RecipeAuditFilterType : FilterInputType<RecipeAudit>
{
    protected override void Configure(IFilterInputTypeDescriptor<RecipeAudit> descriptor) =>
        descriptor
            .Ignore(a => a.RecipeId)
            .Ignore(a => a.RevisionId)
            .Ignore(a => a.ActorId);
}
