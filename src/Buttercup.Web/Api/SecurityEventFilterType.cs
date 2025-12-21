using Buttercup.EntityModel;
using HotChocolate.Data.Filters;

namespace Buttercup.Web.Api;

public sealed class SecurityEventFilterType : FilterInputType<SecurityEvent>
{
    protected override void Configure(IFilterInputTypeDescriptor<SecurityEvent> descriptor) =>
        descriptor.Ignore(e => e.UserId);
}
