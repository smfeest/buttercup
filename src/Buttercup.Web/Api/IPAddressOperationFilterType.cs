using HotChocolate.Data.Filters;

namespace Buttercup.Web.Api;

public class IPAddressOperationFilterInputType
    : FilterInputType, IComparableOperationFilterInputType
{
    protected override void Configure(IFilterInputTypeDescriptor descriptor)
    {
        descriptor.Operation(DefaultFilterOperations.Equals).Type<IPAddressType>();
        descriptor.Operation(DefaultFilterOperations.NotEquals).Type<IPAddressType>();
        descriptor.Operation(DefaultFilterOperations.In).Type<ListType<IPAddressType>>();
        descriptor.Operation(DefaultFilterOperations.NotIn).Type<ListType<IPAddressType>>();
        descriptor.AllowAnd(false).AllowOr(false);
    }
}
