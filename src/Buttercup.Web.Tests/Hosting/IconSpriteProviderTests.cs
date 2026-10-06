using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Buttercup.Web.Hosting;

public sealed class IconSpriteProviderTests : IDisposable
{
    private readonly MemoryCache cache = new(Options.Create(new MemoryCacheOptions()));
    private readonly Mock<IFileProvider> fileProviderMock = new();
    private readonly Mock<IWebHostEnvironment> hostEnvironmentMock = new();
    private readonly FakeLogger<IconSpriteProvider> logger = new();

    private readonly IconSpriteProvider iconSpriteProvider;

    public IconSpriteProviderTests()
    {
        this.fileProviderMock.SetReturnsDefault<IChangeToken>(NullChangeToken.Singleton);
        this.hostEnvironmentMock
            .SetupGet(x => x.ContentRootFileProvider)
            .Returns(this.fileProviderMock.Object);

        this.iconSpriteProvider = new(this.cache, this.hostEnvironmentMock.Object, this.logger);
    }

    public void Dispose() => this.cache.Dispose();

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task GetSprite_CachesEachSprite(string environmentName)
    {
        this.SetupEnvironmentName(environmentName);

        this.SetupGetFileInfo("foo");
        this.SetupGetFileInfo("bar");

        Assert.Equal("<svg>foo:0</svg>", await this.iconSpriteProvider.GetSprite("foo"));
        Assert.Equal(1, this.logger.Collector.Count);
        Assert.Equal("Caching sprite 'foo'", this.logger.Collector.LatestRecord.Message);

        Assert.Equal("<svg>foo:0</svg>", await this.iconSpriteProvider.GetSprite("foo"));
        Assert.Equal(1, this.logger.Collector.Count);

        Assert.Equal("<svg>bar:0</svg>", await this.iconSpriteProvider.GetSprite("bar"));
        Assert.Equal(2, this.logger.Collector.Count);
        Assert.Equal("Caching sprite 'bar'", this.logger.Collector.LatestRecord.Message);
    }

    [Fact]
    public async Task GetSprite_Production_DoesNotWatchForChanges()
    {
        this.SetupEnvironmentName("Production");
        this.SetupGetFileInfo("foo");

        await this.iconSpriteProvider.GetSprite("foo");

        this.fileProviderMock.Verify(x => x.Watch(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetSprite_InDevelopment_InvalidatesCacheEntryOnFileChange()
    {
        this.SetupEnvironmentName("Development");
        this.SetupGetFileInfo("foo");
        this.SetupGetFileInfo("bar");

        var barCts = new CancellationTokenSource();
        this.fileProviderMock
            .Setup(x => x.Watch("icons/sprites/bar.svg"))
            .Returns(new CancellationChangeToken(barCts.Token));

        Assert.Equal("<svg>foo:0</svg>", await this.iconSpriteProvider.GetSprite("foo"));
        Assert.Equal("<svg>bar:0</svg>", await this.iconSpriteProvider.GetSprite("bar"));

        barCts.Cancel();

        Assert.Equal("<svg>foo:0</svg>", await this.iconSpriteProvider.GetSprite("foo"));
        Assert.Equal("<svg>bar:1</svg>", await this.iconSpriteProvider.GetSprite("bar"));
    }

    private void SetupEnvironmentName(string environmentName) =>
        this.hostEnvironmentMock.SetupGet(x => x.EnvironmentName).Returns(environmentName);

    private void SetupGetFileInfo(string spriteName)
    {
        var counter = 0;
        this.fileProviderMock
            .Setup(x => x.GetFileInfo($"icons/sprites/{spriteName}.svg"))
            .Returns(() =>
            {
                var fileInfoMock = new Mock<IFileInfo>();
                var sprite = $"<svg>{spriteName}:{counter++}</svg>";
                fileInfoMock
                    .Setup(x => x.CreateReadStream())
                    .Returns(new MemoryStream(Encoding.UTF8.GetBytes(sprite)));
                return fileInfoMock.Object;
            });
    }
}
