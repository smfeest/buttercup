using System.Text;
using Xunit;

namespace Buttercup.Web.Infrastructure;

public sealed class AssetManifestReaderTests
{
    [Fact]
    public void ReturnsManifestDeserializedFromJson()
    {
        var bytes = Encoding.UTF8.GetBytes("{\"alpha\":\"beta\",\"gamma\":\"delta\"}");

        using var stream = new MemoryStream(bytes);

        var expected = new Dictionary<string, string>
        {
            ["alpha"] = "beta",
            ["gamma"] = "delta",
        };

        var actual = new AssetManifestReader().ReadManifest(stream);

        Assert.Equal(expected, actual);
    }
}
