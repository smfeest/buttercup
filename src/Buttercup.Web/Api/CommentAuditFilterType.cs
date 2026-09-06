using Buttercup.EntityModel;
using HotChocolate.Data.Filters;

namespace Buttercup.Web.Api;

public sealed class CommentAuditFilterType : FilterInputType<CommentAudit>
{
    protected override void Configure(IFilterInputTypeDescriptor<CommentAudit> descriptor) =>
        descriptor
            .Ignore(a => a.CommentId)
            .Ignore(a => a.RevisionId)
            .Ignore(a => a.ActorId);
}
