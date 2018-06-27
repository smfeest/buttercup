namespace Buttercup.Web.Authentication
{
    /// <summary>
    /// Defines the contract for the random token generator.
    /// </summary>
    public interface IRandomTokenGenerator
    {
        /// <summary>
        /// Generates a new token.
        /// </summary>
        /// <returns>
        /// The randomly generated token (48 URL-safe characters).
        /// </returns>
        string Generate();
    }
}
