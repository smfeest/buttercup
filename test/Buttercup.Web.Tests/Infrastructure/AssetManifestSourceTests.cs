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
                var path = Path.Combine("prod-assets", "manifest.json");

                var stream = Mock.Of<Stream>();
                var fileInfo = Mock.Of<IFileInfo>(x => x.CreateReadStream() == stream);
                var fileProvider = Mock.Of<IFileProvider>(x => x.GetFileInfo(path) == fileInfo);
                var hostEnvironment = Mock.Of<IWebHostEnvironment>(
                    x => x.WebRootFileProvider == fileProvider);
                var logger = Mock.Of<ILogger<AssetManifestSource>>();

                this.MockManifestReader
                    .Setup(x => x.ReadManifest(stream))
                    .Returns(this.ExpectedManifest);

                this.ManifestSource = new AssetManifestSource(
                    hostEnvironment, logger, this.MockManifestReader.Object);
            }

            public IDictionary<string, string> ExpectedManifest { get; } =
                new Dictionary<string, string>();

            public AssetManifestSource ManifestSource { get; }

            public Mock<IAssetManifestReader> MockManifestReader { get; } = new();
        }
    }
}
