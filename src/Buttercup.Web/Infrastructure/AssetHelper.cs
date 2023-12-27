using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;

namespace Buttercup.Web.Infrastructure;

public sealed class AssetHelper(
    IAssetManifestSource assetManifestSource,
    IOptions<AssetHelperOptions> options,
    IUrlHelperFactory urlHelperFactory)
    : IAssetHelper
{
    private readonly IAssetManifestSource assetManifestSource = assetManifestSource;
    private readonly bool useProductionAssets = options.Value.UseProductionAssets;
    private readonly IUrlHelperFactory urlHelperFactory = urlHelperFactory;

    public string Url(ActionContext context, string path)
    {
        string contentPath;

        if (this.useProductionAssets)
        {
            var physicalPath = this.assetManifestSource.ProductionManifest[path];

            contentPath = $"~/prod-assets/{physicalPath}";
        }
        else
        {
            contentPath = $"~/assets/{path}";
        }

        var urlHelper = this.urlHelperFactory.GetUrlHelper(context);

        return urlHelper.Content(contentPath)!;
    }
}
