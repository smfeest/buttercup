using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public class SecurityEventType : ObjectType<SecurityEvent>
{
    protected override void Configure(IObjectTypeDescriptor<SecurityEvent> descriptor) =>
        descriptor.Ignore(u => u.UserId);
}
