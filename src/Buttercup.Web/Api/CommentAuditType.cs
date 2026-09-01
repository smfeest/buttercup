using Buttercup.EntityModel;
using Buttercup.Web.Security;

namespace Buttercup.Web.Api;

public sealed class CommentAuditType : ObjectType<CommentAudit>
{
    protected override void Configure(IObjectTypeDescriptor<CommentAudit> descriptor)
    {
        descriptor.Field(r => r.IpAddress).Authorize(AuthorizationPolicyNames.AdminOnly);

        descriptor
            .Ignore(r => r.CommentId)
            .Ignore(r => r.RevisionId)
            .Ignore(r => r.ActorId);
    }
}
