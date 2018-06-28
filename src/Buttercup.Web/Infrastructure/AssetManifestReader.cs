using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Buttercup.Web.Infrastructure
{
    public class AssetManifestReader : IAssetManifestReader
    {
        public IDictionary<string, string> ReadManifest(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                return new JsonSerializer().Deserialize<Dictionary<string, string>>(jsonReader);
            }
        }
    }
}
