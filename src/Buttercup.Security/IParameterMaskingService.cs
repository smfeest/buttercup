namespace Buttercup.Security;

/// <summary>
/// Defines the contract for the parameter masking service.
/// </summary>
public interface IParameterMaskingService
{
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
