using System.Collections.ObjectModel;

namespace Buttercup.Web.Infrastructure;

public sealed class AssetManifestSource(
    IWebHostEnvironment hostEnvironment,
    ILogger<AssetManifestSource> logger,
    IAssetManifestReader manifestReader)
    : IAssetManifestSource
{
    private readonly Lazy<IDictionary<string, string>> productionManifest = new(() =>
    {
        var path = System.IO.Path.Combine("prod-assets", "manifest.json");

        LoggerMessages.LoadingManifest(logger, path, null);

        var fileInfo = hostEnvironment.WebRootFileProvider.GetFileInfo(path);

        using var stream = fileInfo.CreateReadStream();

        return new ReadOnlyDictionary<string, string>(manifestReader.ReadManifest(stream));
    });

    public IDictionary<string, string> ProductionManifest => this.productionManifest.Value;

    private static class LoggerMessages
    {
        public static readonly Action<ILogger, string, Exception?> LoadingManifest =
            LoggerMessage.Define<string>(
                LogLevel.Information, 100, "Loading asset manifest {Path}");
    }
}
