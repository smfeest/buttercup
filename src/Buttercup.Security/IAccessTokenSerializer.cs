namespace Buttercup.Security;

/// <summary>
/// Defines the contract for the access token serializer.
/// </summary>
public interface IAccessTokenSerializer
{
    /// <summary>
    /// Serializes an access token's payload.
    /// </summary>
    /// <param name="payload">
    /// The payload.
    /// </param>
    /// <returns>
    /// The serialized payload.
    /// </returns>
    byte[] Serialize(AccessTokenPayload payload);

    /// <summary>
    /// Deserializes an access token's payload.
    /// </summary>
    /// <param name="data">
    /// The serialized payload.
    /// </param>
    /// <returns>
    /// The deserialized payload.
    /// </returns>
    AccessTokenPayload Deserialize(byte[] data);
}
