using Buttercup.EntityModel;
using HotChocolate.Data.Sorting;

namespace Buttercup.Web.Api;

public sealed class UserAuditEntrySortType : SortInputType<UserAuditEntry>
{
    protected override void Configure(ISortInputTypeDescriptor<UserAuditEntry> descriptor) =>
        descriptor
            .Ignore(e => e.TargetId)
            .Ignore(e => e.ActorId);
}
