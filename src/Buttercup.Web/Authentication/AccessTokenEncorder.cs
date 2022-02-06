using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;

namespace Buttercup.Web.Authentication;

public sealed class AccessTokenEncoder : IAccessTokenEncoder
{
    private readonly IAccessTokenSerializer accessTokenSerializer;
    private readonly IDataProtector dataProtector;

    public AccessTokenEncoder(
        IAccessTokenSerializer accessTokenSerializer,
        IDataProtectionProvider dataProtectionProvider)
    {
        this.accessTokenSerializer = accessTokenSerializer;
        this.dataProtector = dataProtectionProvider.CreateProtector(nameof(AccessTokenEncoder));
    }

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
