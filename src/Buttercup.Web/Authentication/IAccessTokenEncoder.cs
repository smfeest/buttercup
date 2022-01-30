using System.Security.Cryptography;

namespace Buttercup.Web.Authentication;

/// <summary>
/// Defines the contract for the access token encoder.
/// </summary>
public interface IAccessTokenEncoder
{
    /// <summary>
    /// Encodes an access token payload into an encrypted access token.
    /// </summary>
    /// <param name="payload">
    /// The payload.
    /// </param>
    /// <returns>
    /// The encrypted token.
    /// </returns>
    string Encode(AccessTokenPayload payload);

    /// <summary>
    /// Decodes an encrypted access token.
    /// </summary>
    /// <param name="token">
    /// The encrypted token.
    /// </param>
    /// <returns>
    /// The payload.
    /// </returns>
    /// <exception cref="FormatException">
    /// The encrypted token is not base64url encoded.
    /// </exception>
    /// <exception cref="CryptographicException">
    /// The token is malformed or encrypted with the wrong key.
    /// </exception>
    AccessTokenPayload Decode(string token);
}
