using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public class UserAuditEntryType : ObjectType<UserAuditEntry>
{
    protected override void Configure(IObjectTypeDescriptor<UserAuditEntry> descriptor) =>
        descriptor
            .Ignore(u => u.TargetId)
            .Ignore(u => u.ActorId);
}
