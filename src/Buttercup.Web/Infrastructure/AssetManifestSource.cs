using System.Collections.ObjectModel;
using Microsoft.Extensions.FileProviders;

namespace Buttercup.Web.Infrastructure
{
    public class AssetManifestSource : IAssetManifestSource
    {
        private readonly IFileProvider fileProvider;
        private readonly IAssetManifestReader manifestReader;
        private readonly object loadLock = new();
        private readonly ILogger logger;
        private IDictionary<string, string>? productionManifest;

        public AssetManifestSource(
            IWebHostEnvironment hostEnvironment,
            ILogger<AssetManifestSource> logger,
            IAssetManifestReader manifestReader)
        {
            this.fileProvider = hostEnvironment.WebRootFileProvider;
            this.logger = logger;
            this.manifestReader = manifestReader;
        }

        public IDictionary<string, string> ProductionManifest
        {
            get
            {
                if (this.productionManifest == null)
                {
                    lock (this.loadLock)
                    {
                        if (this.productionManifest == null)
                        {
                            var path = Path.Combine("prod-assets", "manifest.json");

                            LoggerMessages.LoadingManifest(this.logger, path, null);

                            var fileInfo = this.fileProvider.GetFileInfo(path);

                            using var stream = fileInfo.CreateReadStream();

                            this.productionManifest = new ReadOnlyDictionary<string, string>(
                                this.manifestReader.ReadManifest(stream));
                        }
                    }
                }

                return this.productionManifest;
            }
        }

        private static class LoggerMessages
        {
            public static readonly Action<ILogger, string, Exception?> LoadingManifest =
                LoggerMessage.Define<string>(
                    LogLevel.Information, 100, "Loading asset manifest {Path}");
        }
    }
}
