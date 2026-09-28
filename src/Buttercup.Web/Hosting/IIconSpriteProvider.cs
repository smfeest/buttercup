namespace Buttercup.Web.Hosting;

/// <summary>
/// Defines the contract for the icon sprite provider.
/// </summary>
public interface IIconSpriteProvider
{
    /// <summary>
    /// Gets an icon sprite.
    /// </summary>
    /// <param name="name">
    /// The sprite name.
    /// </param>
    /// <returns>
    /// The sprite.
    /// </returns>
    Task<string> GetSprite(string name);
}
