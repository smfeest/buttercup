using System.Collections.Generic;
using System.IO;
using Buttercup.Web.TestUtils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Buttercup.Web.Infrastructure
{
    public class AssetManifestSourceTests
    {
        #region ProductionManifest

        [Fact]
        public void ProductionManifestLogsManifestLocation()
        {
            var fixture = new AssetManifestSourceFixture();

            _ = fixture.ManifestSource.ProductionManifest;

            fixture.Logger.AssertSingleEntry(
                LogLevel.Information, $"Loading asset manifest {fixture.ManifestPath}");
        }

        [Fact]
        public void ProductionManifestReturnsManifestAsReadOnlyDictionary()
        {
            var fixture = new AssetManifestSourceFixture();

            var actual = fixture.ManifestSource.ProductionManifest;

            Assert.True(actual.IsReadOnly);
            Assert.Equal(fixture.ExpectedManifest, actual);
        }

        [Fact]
        public void ProductionManifestCachesResult()
        {
            var fixture = new AssetManifestSourceFixture();

            Assert.Same(
                fixture.ManifestSource.ProductionManifest,
                fixture.ManifestSource.ProductionManifest);
            fixture.MockManifestReader.Verify(
                x => x.ReadManifest(It.IsAny<Stream>()), Times.Once());
        }

        #endregion

        private class AssetManifestSourceFixture
        {
            public AssetManifestSourceFixture()
            {
                var stream = Mock.Of<Stream>();
                var fileInfo = Mock.Of<IFileInfo>(x => x.CreateReadStream() == stream);
                var fileProvider = Mock.Of<IFileProvider>(
                    x => x.GetFileInfo(this.ManifestPath) == fileInfo);
                var hostEnvironment = Mock.Of<IWebHostEnvironment>(
                    x => x.WebRootFileProvider == fileProvider);
                this.MockManifestReader
                    .Setup(x => x.ReadManifest(stream))
                    .Returns(this.ExpectedManifest);

                this.ManifestSource = new AssetManifestSource(
                    hostEnvironment, this.Logger, this.MockManifestReader.Object);
            }

            public IDictionary<string, string> ExpectedManifest { get; } =
                new Dictionary<string, string>();

            public AssetManifestSource ManifestSource { get; }

            public ListLogger<AssetManifestSource> Logger { get; } = new();

            public Mock<IAssetManifestReader> MockManifestReader { get; } = new();

            public string ManifestPath { get; } = Path.Combine("prod-assets", "manifest.json");
        }
    }
}
