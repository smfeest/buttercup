using System.Net;
using HotChocolate.Language;
using HotChocolate.Types;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class IPAddressTypeTests
{
    private readonly IPAddressType ipAddressType = new();

    #region IsInstanceOfType(object?)

    [Fact]
    public void IsInstanceOfType_ReturnsTrueForNull() =>
        Assert.True(this.ipAddressType.IsInstanceOfType((object?)null));

    [Fact]
    public void IsInstanceOfType_ReturnsTrueForIpAddressObject() =>
        Assert.True(this.ipAddressType.IsInstanceOfType(new IPAddress(2571693948)));

    [Theory]
    [InlineData("7pUG")]
    [InlineData(2.18)]
    public void IsInstanceOfType_ReturnsFalseForOtherRuntimeType(object value) =>
        Assert.False(this.ipAddressType.IsInstanceOfType(value));

    #endregion

    #region IsInstanceOfType(IValueNode)

    [Fact]
    public void IsInstanceOfType_ReturnsTrueForNullValueNode() =>
        Assert.True(this.ipAddressType.IsInstanceOfType(NullValueNode.Default));

    [Theory]
    [InlineData("115.37.110.113")]
    [InlineData("8818:e7a5:cdfc:8a78:2832:24fd:ab8f:af20")]
    [InlineData("47a2:6d3e:0777::916:d2ad:bbef")]
    [InlineData("::edbe")]
    public void IsInstanceOfType_ReturnsTrueForValidStringValueNode(string stringValue) =>
        Assert.True(this.ipAddressType.IsInstanceOfType(new StringValueNode(stringValue)));

    [Theory]
    [InlineData("139.13.267.138")]
    [InlineData("LMYy")]
    public void IsInstanceOfType_ReturnsFalseForInvalidStringValueNode(string stringValue) =>
        Assert.False(this.ipAddressType.IsInstanceOfType(new StringValueNode(stringValue)));

    [Fact]
    public void IsInstanceOfType_ReturnsFalseForOtherValueNodeType() =>
        Assert.False(this.ipAddressType.IsInstanceOfType(new FloatValueNode(171.12)));

    #endregion

    #region ParseResult

    [Fact]
    public void ParseResult_ReturnsNullValueNodeForNull() =>
        Assert.Equal(NullValueNode.Default, this.ipAddressType.ParseResult(null));

    [Theory]
    [InlineData("165.41.250.5")]
    [InlineData("7e12:7ebe:4935:8144:74b1:1d60:16ca:8435")]
    public void ParseResult_ReturnsStringValueNodeForIPAddressObject(string ipAddressString)
    {
        var ipAddress = IPAddress.Parse(ipAddressString);
        var valueNode = this.ipAddressType.ParseResult(ipAddress);
        var stringValueNode = Assert.IsType<StringValueNode>(valueNode);
        Assert.Equal(ipAddressString, stringValueNode.Value);
    }

    [Fact]
    public void ParseResult_ThrowsForOtherResultType() =>
        Assert.Throws<SerializationException>(() => this.ipAddressType.ParseResult(new DateTime()));

    #endregion

    #region ParseLiteral

    [Fact]
    public void ParseLiteral_ReturnsNullForNullValueNode() =>
        Assert.Null(this.ipAddressType.ParseLiteral(NullValueNode.Default));

    [Theory]
    [InlineData("153.72.239.124")]
    [InlineData("::4bf5")]
    public void ParseLiteral_ReturnsIPAddressForValidStringValueNode(string stringValue) =>
        Assert.Equal(
            IPAddress.Parse(stringValue),
            this.ipAddressType.ParseLiteral(new StringValueNode(stringValue)));

    [Fact]
    public void ParseResult_ThrowsForInvalidStringValueNode() =>
        Assert.Throws<SerializationException>(
            () => this.ipAddressType.ParseResult(new StringValueNode("A1rq")));

    [Fact]
    public void ParseResult_ThrowsForOtherValueNodeType() =>
        Assert.Throws<SerializationException>(
            () => this.ipAddressType.ParseResult(new BooleanValueNode(true)));

    #endregion

    #region ParseValue

    [Fact]
    public void ParseValue_ReturnsNullValueNodeForNull() =>
        Assert.Equal(NullValueNode.Default, this.ipAddressType.ParseValue(null));

    [Theory]
    [InlineData("113.254.5.185")]
    [InlineData("4885:f007:b37b::d272:a068:8053")]
    public void ParseValue_ReturnsStringValueNodeForIPAddressObject(string ipAddressString)
    {
        var ipAddress = IPAddress.Parse(ipAddressString);
        var valueNode = this.ipAddressType.ParseValue(ipAddress);
        var stringValueNode = Assert.IsType<StringValueNode>(valueNode);
        Assert.Equal(ipAddressString, stringValueNode.Value);
    }

    [Fact]
    public void ParseValue_ThrowsForOtherValueType() =>
        Assert.Throws<SerializationException>(() => this.ipAddressType.ParseValue("9etE"));

    #endregion

    #region TrySerialize

    [Fact]
    public void TrySerialize_PassesThroughNull()
    {
        Assert.True(this.ipAddressType.TrySerialize(null, out var runtimeValue));
        Assert.Null(runtimeValue);
    }

    [Theory]
    [InlineData("35.74.173.89")]
    [InlineData("2dbc:f949:6fdc:fbb1:de57:584:9746:db36")]
    [InlineData("e717:763b::41ad:d10b:49a2:5907")]
    [InlineData("::8428")]
    public void TrySerialize_ConvertsIPAddressObjectToString(string ipAddressStringIn)
    {
        var ipAddress = IPAddress.Parse(ipAddressStringIn);
        Assert.True(this.ipAddressType.TrySerialize(ipAddress, out var runtimeValue));
        var ipAddressStringOut = Assert.IsType<string>(runtimeValue);
        Assert.Equal(ipAddressStringIn, ipAddressStringOut);
    }

    [Theory]
    [InlineData(123)]
    [InlineData(false)]
    public void TrySerialize_RejectsInvalidRuntimeValue(object valueIn)
    {
        Assert.False(this.ipAddressType.TrySerialize(valueIn, out var runtimeValue));
        Assert.Null(runtimeValue);
    }

    #endregion

    #region TryDeserialize

    [Fact]
    public void TryDeserialize_PassesThroughNull()
    {
        Assert.True(this.ipAddressType.TryDeserialize(null, out var runtimeValue));
        Assert.Null(runtimeValue);
    }

    [Fact]
    public void TryDeserialize_PassesThroughIPAddressObject()
    {
        var ipAddressIn = IPAddress.Parse("172.16.0.0");
        Assert.True(this.ipAddressType.TryDeserialize(ipAddressIn, out var runtimeValue));
        var ipAddressOut = Assert.IsType<IPAddress>(runtimeValue);
        Assert.Equal(ipAddressIn, ipAddressOut);
    }

    [Theory]
    [InlineData("159.11.206.115")]
    [InlineData("1048:06da:3bdc:ec4a:a143:775e:4ae2:5333")]
    [InlineData("dd24:145:d65f::41e5:dc8e:471f")]
    [InlineData("::30a")]
    public void TryDeserialize_ParsesValidIpAddressString(string ipAddressString)
    {
        Assert.True(this.ipAddressType.TryDeserialize(ipAddressString, out var runtimeValue));
        var ipAddress = Assert.IsType<IPAddress>(runtimeValue);
        Assert.Equal(IPAddress.Parse(ipAddressString), ipAddress);
    }

    [Theory]
    [InlineData("129.168.1.257")]
    [InlineData("wd9n")]
    [InlineData(123)]
    [InlineData(false)]
    public void TryDeserialize_RejectsInvalidResultValue(object valueIn)
    {
        Assert.False(this.ipAddressType.TryDeserialize(valueIn, out var runtimeValue));
        Assert.Null(runtimeValue);
    }

    #endregion
}
