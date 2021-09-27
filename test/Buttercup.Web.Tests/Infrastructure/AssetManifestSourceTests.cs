using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Buttercup.Web.Infrastructure
{
    public class AssetManifestSourceTests
    {
        #region ProductionManifest

        [Fact]
        public void ProductionManifestReturnsManifestAsReadOnlyDictionary()
        {
            var context = new Context();

            var actual = context.ManifestSource.ProductionManifest;

            Assert.True(actual.IsReadOnly);
            Assert.Equal(context.ExpectedManifest, actual);
        }

        [Fact]
        public void ProductionManifestCachesResult()
        {
            var context = new Context();

            Assert.Same(
                context.ManifestSource.ProductionManifest,
                context.ManifestSource.ProductionManifest);
            context.MockManifestReader.Verify(
                x => x.ReadManifest(It.IsAny<Stream>()), Times.Once());
        }

        #endregion

        private class Context
        {
            public Context()
            {
                var stream = new MemoryStream();

                var mockFileInfo = new Mock<IFileInfo>();
                mockFileInfo.Setup(x => x.CreateReadStream()).Returns(stream);

                var mockFileProvider = new Mock<IFileProvider>();
                var path = Path.Combine("prod-assets", "manifest.json");
                mockFileProvider.Setup(x => x.GetFileInfo(path)).Returns(mockFileInfo.Object);

                var mockHostEnvironment = new Mock<IWebHostEnvironment>();
                mockHostEnvironment
                    .SetupGet(x => x.WebRootFileProvider)
                    .Returns(mockFileProvider.Object);

                var mockLogger = new Mock<ILogger<AssetManifestSource>>();

                this.MockManifestReader
                    .Setup(x => x.ReadManifest(stream))
                    .Returns(this.ExpectedManifest);

                this.ManifestSource = new AssetManifestSource(
                    mockHostEnvironment.Object,
                    mockLogger.Object,
                    this.MockManifestReader.Object);
            }

            public IDictionary<string, string> ExpectedManifest { get; } =
                new Dictionary<string, string>();

            public AssetManifestSource ManifestSource { get; }

            public Mock<IAssetManifestReader> MockManifestReader { get; } =
                new Mock<IAssetManifestReader>();
        }
    }
}
