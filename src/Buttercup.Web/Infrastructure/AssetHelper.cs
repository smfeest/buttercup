using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Buttercup.Web.Infrastructure
{
    public class AssetHelper : IAssetHelper
    {
        private readonly IAssetManifestSource assetManifestSource;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly IUrlHelperFactory urlHelperFactory;

        public AssetHelper(
            IAssetManifestSource assetManifestSource,
            IHostingEnvironment hostingEnvironment,
            IUrlHelperFactory urlHelperFactory)
        {
            this.assetManifestSource = assetManifestSource;
            this.hostingEnvironment = hostingEnvironment;
            this.urlHelperFactory = urlHelperFactory;
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1055:UriReturnValuesShouldNotBeStrings",
            Justification = "IUrlHelper.Content uses strings for paths")]
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

            if (this.hostingEnvironment.IsDevelopment())
            {
                contentPath = $"~/assets/{path}";
            }
            else
            {
                var physicalPath = this.assetManifestSource.ProductionManifest[path];

                contentPath = $"~/prod-assets/{physicalPath}";
            }

            var urlHelper = this.urlHelperFactory.GetUrlHelper(context);

            return urlHelper.Content(contentPath);
        }
    }
}
