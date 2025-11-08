using Buttercup.Application;

namespace Buttercup.Web.Api;

public sealed class NewUserAttributesInputType : InputObjectType<NewUserAttributes>
{
    protected override void Configure(IInputObjectTypeDescriptor<NewUserAttributes> descriptor)
    {
        descriptor.Ignore(a => a.Password);
        descriptor.Field(a => a.IsAdmin).DefaultValue(false);
    }
}
