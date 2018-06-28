using System.Collections.Generic;

namespace Buttercup.Web.Infrastructure
{
    /// <summary>
    /// Defines the contract for the asset manifest source.
    /// </summary>
    public interface IAssetManifestSource
    {
        /// <summary>
        /// Gets the production asset manifest.
        /// </summary>
        /// <value>
        /// The production asset manifest as a dictionary of physical paths keyed by logical path.
        /// </value>
        IDictionary<string, string> ProductionManifest { get; }
    }
}
