using System.Net;
using System.Text.Json;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Features;
using HotChocolate.Language;
using HotChocolate.Text.Json;
using HotChocolate.Types;
using Moq;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class IPAddressTypeTests
{
    private readonly IPAddressType ipAddressType = new();

    #region CoerceInputLiteral

    [Theory]
    [InlineData("153.72.239.124")]
    [InlineData("::4bf5")]
    public void CoerceInputLiteral_ReturnsIPAddressForValidStringValueNode(string stringValue) =>
        Assert.Equal(
            IPAddress.Parse(stringValue),
            this.ipAddressType.CoerceInputLiteral(new StringValueNode(stringValue)));

    [Theory]
    [InlineData("139.13.267.138")]
    [InlineData("LMYy")]
    public void CoerceInputLiteral_ThrowsForInvalidStringValueNode(string stringValue) =>
        Assert.Throws<LeafCoercionException>(
            () => this.ipAddressType.CoerceInputLiteral(new StringValueNode(stringValue)));

    [Fact]
    public void CoerceInputLiteral_ThrowsForOtherValueNodeType() =>
        Assert.Throws<LeafCoercionException>(
            () => this.ipAddressType.CoerceInputLiteral(new BooleanValueNode(true)));

    #endregion

    #region CoerceInputValue

    [Theory]
    [InlineData("113.254.5.185")]
    [InlineData("4885:f007:b37b::d272:a068:8053")]
    public void CoerceInputValue_ReturnsIPAddressForValidJsonStringElement(string stringValue) =>
        Assert.Equal(
            IPAddress.Parse(stringValue),
            this.ipAddressType.CoerceInputValue(
                JsonSerializer.SerializeToElement(stringValue), Mock.Of<IFeatureProvider>()));

    [Theory]
    [InlineData("::h000")]
    [InlineData("9etE")]
    public void CoerceInputValue_ThrowsForInvalidJsonStringElement(string stringValue) =>
        Assert.Throws<LeafCoercionException>(
            () => this.ipAddressType.CoerceInputValue(
                JsonSerializer.SerializeToElement(stringValue), Mock.Of<IFeatureProvider>()));

    [Fact]
    public void CoerceInputValue_ThrowsForOtherJsonElementType() =>
        Assert.Throws<LeafCoercionException>(
            () => this.ipAddressType.CoerceInputValue(
                JsonSerializer.SerializeToElement(1.23), Mock.Of<IFeatureProvider>()));

    #endregion

    #region CoerceOutputValue

    [Theory]
    [InlineData("35.74.173.89")]
    [InlineData("2dbc:f949:6fdc:fbb1:de57:584:9746:db36")]
    public async Task CoerceOutputValue_ConvertsIPAddressObjectToString(string ipAddressString)
    {
        var ipAddress = IPAddress.Parse(ipAddressString);

        var executor = await new ServiceCollection()
            .AddGraphQL()
            .AddQueryType(d => d
                .Name("Query")
                .Field("value")
                .Type<IPAddressType>()
                .Resolve(IPAddress.Parse(ipAddressString)))
            .BuildRequestExecutorAsync(null, TestContext.Current.CancellationToken);

        var result = await executor.ExecuteAsync(
            "{ value }", TestContext.Current.CancellationToken);

        var data = result.ExpectOperationResult().Data;
        Assert.NotNull(data);
        var resultDocument = Assert.IsType<ResultDocument>(data.Value.Value);
        Assert.Equal(ipAddressString, resultDocument.Data.GetProperty("value").GetString());
    }

    #endregion

    #region ValueToLiteral

    [Theory]
    [InlineData("165.41.250.5")]
    [InlineData("7e12:7ebe:4935:8144:74b1:1d60:16ca:8435")]
    public void ValueToLiteral_ReturnsStringValueNodeForIPAddressObject(string ipAddressString)
    {
        var valueNode = this.ipAddressType.ValueToLiteral(IPAddress.Parse(ipAddressString));
        var stringValueNode = Assert.IsType<StringValueNode>(valueNode);
        Assert.Equal(ipAddressString, stringValueNode.Value);
    }

    [Fact]
    public void ValueToLiteral_ThrowsForOtherResultType() =>
        Assert.Throws<LeafCoercionException>(() => this.ipAddressType.ValueToLiteral(new DateTime()));

    #endregion
}
