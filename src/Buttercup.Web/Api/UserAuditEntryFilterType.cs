using Buttercup.EntityModel;
using HotChocolate.Data.Filters;

namespace Buttercup.Web.Api;

public sealed class UserAuditEntryFilterType : FilterInputType<UserAuditEntry>
{
    protected override void Configure(IFilterInputTypeDescriptor<UserAuditEntry> descriptor) =>
        descriptor
            .Ignore(e => e.TargetId)
            .Ignore(e => e.ActorId)
            .Field(e => e.IpAddress).Type<IPAddressOperationFilterInputType>();
}
