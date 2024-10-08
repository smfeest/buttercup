using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;

namespace Buttercup.Security;

internal sealed class AccessTokenEncoder(
    IAccessTokenSerializer accessTokenSerializer, IDataProtectionProvider dataProtectionProvider)
    : IAccessTokenEncoder
{
    private readonly IAccessTokenSerializer accessTokenSerializer = accessTokenSerializer;
    private readonly IDataProtector dataProtector =
        dataProtectionProvider.CreateProtector(nameof(AccessTokenEncoder));

    public string Encode(AccessTokenPayload payload)
    {
        var payloadData = this.accessTokenSerializer.Serialize(payload);
        var encryptedPayloadData = this.dataProtector.Protect(payloadData);

        return WebEncoders.Base64UrlEncode(encryptedPayloadData);
    }

    public AccessTokenPayload Decode(string token)
    {
        var encryptedPayloadData = WebEncoders.Base64UrlDecode(token);
        var payloadData = this.dataProtector.Unprotect(encryptedPayloadData);

        return this.accessTokenSerializer.Deserialize(payloadData);
    }
}
