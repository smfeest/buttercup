using Buttercup.TestUtils;
using Microsoft.Extensions.FileProviders;
using Moq;
using Xunit;

namespace Buttercup.Web.Infrastructure;

public sealed class AssetManifestSourceTests
{
    private readonly IDictionary<string, string> expectedManifest =
        new Dictionary<string, string>() { ["foo.js"] = "foo-80bef72723.js" };
    private readonly ListLogger<AssetManifestSource> logger = new();
    private readonly string manifestPath = Path.Combine("prod-assets", "manifest.json");
    private readonly Mock<IAssetManifestReader> manifestReaderMock = new();

    private readonly AssetManifestSource manifestSource;

    public AssetManifestSourceTests()
    {
        var stream = Mock.Of<Stream>();
        var fileInfo = Mock.Of<IFileInfo>(x => x.CreateReadStream() == stream);
        var fileProvider = Mock.Of<IFileProvider>(
            x => x.GetFileInfo(this.manifestPath) == fileInfo);
        var hostEnvironment = Mock.Of<IWebHostEnvironment>(
            x => x.WebRootFileProvider == fileProvider);
        this.manifestReaderMock
            .Setup(x => x.ReadManifest(stream))
            .Returns(this.expectedManifest);

        this.manifestSource = new AssetManifestSource(
            hostEnvironment, this.logger, this.manifestReaderMock.Object);
    }

    #region ProductionManifest

    [Fact]
    public void ProductionManifest_LogsManifestLocation()
    {
        _ = this.manifestSource.ProductionManifest;

        LogAssert.HasEntry(
            this.logger, LogLevel.Information, 100, $"Loading asset manifest {this.manifestPath}");
    }

    [Fact]
    public void ProductionManifest_ReturnsManifestAsReadOnlyDictionary()
    {
        var actual = this.manifestSource.ProductionManifest;

        Assert.True(actual.IsReadOnly);
        Assert.Equal(this.expectedManifest, actual);
    }

    [Fact]
    public void ProductionManifest_CachesResult()
    {
        Assert.Same(
            this.manifestSource.ProductionManifest,
            this.manifestSource.ProductionManifest);

        this.manifestReaderMock.Verify(x => x.ReadManifest(It.IsAny<Stream>()), Times.Once());
    }

    #endregion
}
