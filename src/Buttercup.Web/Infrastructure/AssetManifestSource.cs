namespace Buttercup.Web.Infrastructure;

public sealed partial class AssetManifestSource(
    IWebHostEnvironment hostEnvironment,
    ILogger<AssetManifestSource> logger,
    IAssetManifestReader manifestReader)
    : IAssetManifestSource
{
    private readonly Lazy<IReadOnlyDictionary<string, string>> productionManifest = new(() =>
    {
        var path = System.IO.Path.Combine("prod-assets", "manifest.json");

        LogLoadingManifest(logger, path);

        var fileInfo = hostEnvironment.WebRootFileProvider.GetFileInfo(path);

        using var stream = fileInfo.CreateReadStream();

        return manifestReader.ReadManifest(stream).AsReadOnly();
    });

    public IReadOnlyDictionary<string, string> ProductionManifest => this.productionManifest.Value;

    [LoggerMessage(
        EventId = 100,
        EventName = "LoadingManifest",
        Level = LogLevel.Information,
        Message = "Loading asset manifest {Path}")]
    private static partial void LogLoadingManifest(ILogger logger, string path);
}
