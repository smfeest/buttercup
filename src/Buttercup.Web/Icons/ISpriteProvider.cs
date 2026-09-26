using Microsoft.AspNetCore.Html;

namespace Buttercup.Web.Icons;

/// <summary>
/// Defines the contract for the icon sprite provider.
/// </summary>
public interface ISpriteProvider
{
    /// <summary>
    /// The icon sprite.
    /// </summary>
    IHtmlContent Sprite { get; }
}
