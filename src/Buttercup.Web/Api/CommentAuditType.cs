using Buttercup.EntityModel;
using Buttercup.Web.Security;

namespace Buttercup.Web.Api;

public sealed class CommentAuditType : ObjectType<CommentAudit>
{
    protected override void Configure(IObjectTypeDescriptor<CommentAudit> descriptor)
    {
        descriptor.Field(a => a.IpAddress).Authorize(AuthorizationPolicyNames.AdminOnly);

        descriptor
            .Ignore(a => a.CommentId)
            .Ignore(a => a.RevisionId)
            .Ignore(a => a.ActorId);
    }
}
