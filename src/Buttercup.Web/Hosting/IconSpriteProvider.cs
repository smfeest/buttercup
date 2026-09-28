using Microsoft.Extensions.Caching.Memory;

namespace Buttercup.Web.Hosting;

public sealed partial class IconSpriteProvider(
    IMemoryCache cache, IWebHostEnvironment hostEnvironment, ILogger<IconSpriteProvider> logger)
    : IIconSpriteProvider
{
    private readonly IMemoryCache cache = cache;
    private readonly IWebHostEnvironment hostEnvironment = hostEnvironment;
    private readonly ILogger<IconSpriteProvider> logger = logger;

    public async Task<string> GetSprite(string name) =>
        await this.cache.GetOrCreateAsync(
            $"icon-sprite/{name}",
            async (cacheEntry) =>
            {
                this.LogCachingSprite(name);

                var filePath = $"icons/sprites/{name}.svg";
                var fileProvider = this.hostEnvironment.ContentRootFileProvider;

                if (this.hostEnvironment.IsDevelopment())
                {
                    cacheEntry.AddExpirationToken(fileProvider.Watch(filePath));
                }

                var fileInfo = fileProvider.GetFileInfo(filePath);
                using var stream = fileInfo.CreateReadStream();
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }) ??
            throw new InvalidOperationException($"Unexpected null cache entry for sprite '{name}'");

    [LoggerMessage(
        EventId = 1,
        EventName = "CachingSprite",
        Level = LogLevel.Debug,
        Message = "Caching sprite '{Name}'")]
    private partial void LogCachingSprite(string name);
}
