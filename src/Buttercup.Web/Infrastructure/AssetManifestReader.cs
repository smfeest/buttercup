using System.Text.Json;

namespace Buttercup.Web.Infrastructure;

public sealed class AssetManifestReader : IAssetManifestReader
{
    public IDictionary<string, string> ReadManifest(Stream stream)
    {
        using var streamReader = new StreamReader(stream);

        return JsonSerializer.Deserialize<Dictionary<string, string>>(
            streamReader.ReadToEnd())!;
    }
}
