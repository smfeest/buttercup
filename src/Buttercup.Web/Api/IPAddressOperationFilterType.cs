using HotChocolate.Data.Filters;

namespace Buttercup.Web.Api;

public class IPAddressOperationFilterInputType
    : FilterInputType, IComparableOperationFilterInputType
{
    protected override void Configure(IFilterInputTypeDescriptor descriptor)
    {
        descriptor.Operation(DefaultFilterOperations.Equals).Type<StringType>();
        descriptor.Operation(DefaultFilterOperations.NotEquals).Type<StringType>();
        descriptor.Operation(DefaultFilterOperations.In).Type<ListType<StringType>>();
        descriptor.Operation(DefaultFilterOperations.NotIn).Type<ListType<StringType>>();
        descriptor.AllowAnd(false).AllowOr(false);
    }
}
