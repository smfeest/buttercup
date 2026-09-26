using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Buttercup.Web.Icons;

public sealed partial class SpriteProvider : ISpriteProvider
{
    private const string SpritePath = "Icons/sprite.svg";

    private volatile IChangeToken changeToken = NullChangeToken.Singleton;
    private readonly IFileProvider fileProvider;
    private readonly Lock loadLock = new();
    private readonly ILogger<SpriteProvider> logger;
    private volatile IHtmlContent sprite = HtmlString.Empty;
    private readonly bool watchEnabled;

    public SpriteProvider(IWebHostEnvironment hostEnvironment, ILogger<SpriteProvider> logger)
    {
        this.fileProvider = hostEnvironment.ContentRootFileProvider;
        this.watchEnabled = hostEnvironment.IsDevelopment();
        this.logger = logger;
        this.Load();
    }

    public IHtmlContent Sprite
    {
        get
        {
            if (this.changeToken.HasChanged)
            {
                lock (this.loadLock)
                {
                    if (this.changeToken.HasChanged)
                    {
                        this.Load();
                    }
                }
            }
            return this.sprite;
        }
    }

    private void Load()
    {
        if (this.watchEnabled)
        {
            this.changeToken = this.fileProvider.Watch(SpritePath);
        }

        try
        {
            var fileInfo = this.fileProvider.GetFileInfo(SpritePath);
            using var stream = fileInfo.CreateReadStream();
            using var reader = new StreamReader(stream);
            this.sprite = new HtmlString(reader.ReadToEnd());
        }
        catch (IOException e)
        {
            this.LogLoadError(e);
        }
    }

    [LoggerMessage(
        EventId = 1,
        EventName = "LoadError",
        Level = LogLevel.Error,
        Message = "Unable to load icon sprite")]
    private partial void LogLoadError(Exception exception);
}
