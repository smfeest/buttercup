namespace Buttercup.Security;

/// <summary>
/// Defines the contract for the parameter masking service.
/// </summary>
public interface IParameterMaskingService
{
    /// <summary>
    /// Masks an email address for inclusion in a log or exception message.
    /// </summary>
    /// <remarks>
    /// The returned value exposes the first and last couple of characters of username and domain
    /// name. If the input string does not contain an '@' character (and is therefore not a valid
    /// email address) then the first and last couple of characters of the entire string are
    /// exposed. Parts containing fewer than six characters are masked in their entirety.
    /// </remarks>
    /// <param name="email">
    /// The email address to mask.
    /// </param>
    /// <returns>
    /// The masked email address.
    /// </returns>
    string MaskEmail(string email);

    /// <summary>
    /// Masks a token for inclusion in a log or exception message.
    /// </summary>
    /// <remarks>
    /// The masked token only exposes the first six characters of the input token. If the input
    /// token contains six or fewer characters then it is returned as-is. This is considered safe on
    /// the basis that all genuine tokens masked using this method contain considerably more than
    /// six characters.
    /// </remarks>
    /// <param name="token">
    /// The token to mask.
    /// </param>
    /// <returns>
    /// The masked token.
    /// </returns>
    string MaskToken(string token);
}
