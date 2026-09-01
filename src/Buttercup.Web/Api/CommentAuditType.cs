using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed class CommentAuditType : ObjectType<CommentAudit>
{
    protected override void Configure(IObjectTypeDescriptor<CommentAudit> descriptor) =>
        descriptor
            .Ignore(a => a.CommentId)
            .Ignore(a => a.RevisionId)
            .Ignore(a => a.ActorId);
}
