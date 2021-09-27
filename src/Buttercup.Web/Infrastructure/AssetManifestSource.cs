using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Buttercup.Web.Infrastructure
{
    public class AssetManifestSource : IAssetManifestSource
    {
        private readonly IFileProvider fileProvider;
        private readonly IAssetManifestReader manifestReader;
        private readonly object loadLock = new object();
        private readonly ILogger logger;
        private IDictionary<string, string> productionManifest;

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

                            this.logger.LogInformation("Loading asset manifest {path}", path);

                            var fileInfo = this.fileProvider.GetFileInfo(path);

                            using (var stream = fileInfo.CreateReadStream())
                            {
                                this.productionManifest = new ReadOnlyDictionary<string, string>(
                                    this.manifestReader.ReadManifest(stream));
                            }
                        }
                    }
                }

                return this.productionManifest;
            }
        }
    }
}
