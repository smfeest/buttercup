using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace Buttercup.Web.Infrastructure
{
    /// <summary>
    /// Defines the contract for the asset helper.
    /// </summary>
    public interface IAssetHelper
    {
        /// <summary>
        /// Gets the URL for an asset, relative to the web root of the application.
        /// </summary>
        /// <remarks>
        /// In development, this method simply prepends `assets/` to <paramref name="path" />.
        /// In all other environments, it prepends `prod-assets/` to the corresponding path in the
        /// production asset manifest.
        /// </remarks>
        /// <param name="context">
        /// The action context.
        /// </param>
        /// <param name="path">
        /// The asset's logical path, relative to the root of the assets directory.
        /// </param>
        /// <returns>
        /// The URL for the asset, relative to the web root of the application.
        /// </returns>
        [SuppressMessage(
            "Design",
            "CA1055:UriReturnValuesShouldNotBeStrings",
            Justification = "IUrlHelper.Content uses strings for paths")]
        string Url(ActionContext context, string path);
    }
}
