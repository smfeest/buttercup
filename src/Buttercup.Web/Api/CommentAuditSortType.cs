using Buttercup.EntityModel;
using HotChocolate.Data.Sorting;

namespace Buttercup.Web.Api;

public sealed class CommentAuditSortType : SortInputType<CommentAudit>
{
    protected override void Configure(ISortInputTypeDescriptor<CommentAudit> descriptor) =>
        descriptor
            .Ignore(a => a.CommentId)
            .Ignore(a => a.RevisionId)
            .Ignore(a => a.ActorId);
}
