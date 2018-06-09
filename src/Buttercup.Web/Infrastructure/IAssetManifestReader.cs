using System.Collections.Generic;
using System.IO;

namespace Buttercup.Web.Infrastructure
{
    /// <summary>
    /// Defines the contract for the asset manifest reader.
    /// </summary>
    public interface IAssetManifestReader
    {
        /// <summary>
        /// Reads an asset manifest from a stream.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <returns>
        /// The manifest's contents as a dictionary of physical paths keyed by logical path.
        /// </returns>
        IDictionary<string, string> ReadManifest(Stream stream);
    }
}
