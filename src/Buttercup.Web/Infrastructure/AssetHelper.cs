using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Hosting;

namespace Buttercup.Web.Infrastructure
{
    public class AssetHelper : IAssetHelper
    {
        private readonly IAssetManifestSource assetManifestSource;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly IUrlHelperFactory urlHelperFactory;

        public AssetHelper(
            IAssetManifestSource assetManifestSource,
            IWebHostEnvironment hostEnvironment,
            IUrlHelperFactory urlHelperFactory)
        {
            this.assetManifestSource = assetManifestSource;
            this.hostEnvironment = hostEnvironment;
            this.urlHelperFactory = urlHelperFactory;
        }

        public string Url(ActionContext context, string path)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            string contentPath;

            if (this.hostEnvironment.IsDevelopment())
            {
                contentPath = $"~/assets/{path}";
            }
            else
            {
                var physicalPath = this.assetManifestSource.ProductionManifest[path];

                contentPath = $"~/prod-assets/{physicalPath}";
            }

            var urlHelper = this.urlHelperFactory.GetUrlHelper(context);

            return urlHelper.Content(contentPath)!;
        }
    }
}
