using Buttercup.EntityModel;
using HotChocolate.Data.Sorting;

namespace Buttercup.Web.Api;

public sealed class SecurityEventSortType : SortInputType<SecurityEvent>
{
    protected override void Configure(ISortInputTypeDescriptor<SecurityEvent> descriptor) =>
        descriptor
            .Ignore(e => e.Time)
            .Ignore(e => e.UserId);
}
