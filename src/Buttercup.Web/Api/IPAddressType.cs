using System.Net;
using System.Text.Json;
using HotChocolate.Features;
using HotChocolate.Language;
using HotChocolate.Text.Json;

namespace Buttercup.Web.Api;

public sealed class IPAddressType : ScalarType<IPAddress, StringValueNode>
{
    public IPAddressType() : base("IPAddress", BindingBehavior.Implicit) =>
        this.Description = "The `IPAddress` scalar type represents an IPv4 or IPv6 address";

    protected override IPAddress OnCoerceInputLiteral(StringValueNode valueLiteral) =>
        IPAddress.Parse(valueLiteral.Value);

    protected override IPAddress OnCoerceInputValue(
        JsonElement inputValue, IFeatureProvider context) =>
        IPAddress.Parse(inputValue.GetString()!);

    protected override void OnCoerceOutputValue(
        IPAddress runtimeValue, ResultElement resultValue) =>
        resultValue.SetStringValue(runtimeValue.ToString());

    protected override StringValueNode OnValueToLiteral(IPAddress runtimeValue) =>
        new(runtimeValue.ToString());
}
