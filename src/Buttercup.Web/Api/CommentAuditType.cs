using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed class CommentAuditType : ObjectType<CommentAudit>
{
    protected override void Configure(IObjectTypeDescriptor<CommentAudit> descriptor) =>
        descriptor
            .Ignore(r => r.CommentId)
            .Ignore(r => r.RevisionId)
            .Ignore(r => r.ActorId);
}
