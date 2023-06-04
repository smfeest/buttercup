using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Buttercup.Web.Infrastructure;

public sealed class AssetHelper : IAssetHelper
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
