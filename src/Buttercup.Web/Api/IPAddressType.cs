using System.Net;
using HotChocolate.Language;

namespace Buttercup.Web.Api;

public sealed class IPAddressType : ScalarType<IPAddress, StringValueNode>
{
    public IPAddressType() : base("IPAddress", BindingBehavior.Implicit) =>
        this.Description = "The `IPAddress` scalar type represents an IPv4 or IPv6 address";

    protected override bool IsInstanceOfType(StringValueNode valueSyntax) =>
        IPAddress.TryParse(valueSyntax.Value, out _);

    public override IValueNode ParseResult(object? resultValue) => this.ParseValue(resultValue);

    protected override IPAddress ParseLiteral(StringValueNode valueSyntax) =>
        IPAddress.Parse(valueSyntax.Value);

    protected override StringValueNode ParseValue(IPAddress runtimeValue) =>
        new(runtimeValue.ToString());

    public override bool TrySerialize(object? runtimeValue, out object? resultValue)
    {
        if (runtimeValue is null)
        {
            resultValue = null;
            return true;
        }

        if (runtimeValue is IPAddress ipAddress)
        {
            resultValue = ipAddress.ToString();
            return true;
        }

        resultValue = null;
        return false;
    }

    public override bool TryDeserialize(object? resultValue, out object? runtimeValue)
    {
        if (resultValue is null)
        {
            runtimeValue = null;
            return true;
        }

        if (resultValue is IPAddress ipAddress)
        {
            runtimeValue = ipAddress;
            return true;
        }

        if (resultValue is string resultString &&
            IPAddress.TryParse(resultString, out var parsedIpAddress))
        {
            runtimeValue = parsedIpAddress;
            return true;
        }

        runtimeValue = null;
        return false;
    }
}
