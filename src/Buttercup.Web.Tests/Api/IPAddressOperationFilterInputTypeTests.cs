using HotChocolate;
using HotChocolate.Data.Filters;
using HotChocolate.Types;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class IPAddressOperationFilterInputTypeTests
{
    [Fact]
    public void HasEqualityAndSetOperations()
    {
        var type = SchemaBuilder
            .New()
            .AddFiltering()
            .ModifyOptions(o => o.StrictValidation = false)
            .AddType<IPAddressOperationFilterInputType>()
            .Create()
            .GetType<IFilterInputType>("IPAddressOperationFilterInput");

        Assert.Collection(type.Fields,
            field =>
            {
                Assert.Equal("eq", field.Name);
                Assert.IsType<IPAddressType>(field.Type);
            },
            field =>
            {
                Assert.Equal("neq", field.Name);
                Assert.IsType<IPAddressType>(field.Type);
            },
            field =>
            {
                Assert.Equal("in", field.Name);
                var listType = Assert.IsType<ListType>(field.Type);
                Assert.IsType<IPAddressType>(listType.ElementType);
            },
            field =>
            {
                Assert.Equal("nin", field.Name);
                var listType = Assert.IsType<ListType>(field.Type);
                Assert.IsType<IPAddressType>(listType.ElementType);
            });
    }
}
