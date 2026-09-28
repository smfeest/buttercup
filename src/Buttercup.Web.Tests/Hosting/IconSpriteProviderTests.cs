// using Microsoft.Extensions.Caching.Memory;
// using Microsoft.Extensions.Logging.Testing;
// using Microsoft.Extensions.Options;
// using Moq;

// namespace Buttercup.Web.Hosting;

// public sealed class IconSpriteProviderTests
// {
//     private readonly MemoryCache cache = new(Options.Create(new MemoryCacheOptions()));
//     private readonly Mock<IWebHostEnvironment> hostEnvironmentMock = new();
//     private readonly FakeLogger<IconSpriteProvider> logger = new();

//     private readonly IconSpriteProvider iconSpriteProvider;

//     public IconSpriteProviderTests() =>
//         this.iconSpriteProvider = new(this.cache, this.hostEnvironmentMock.Object, this.logger);

//     // [Fact]
//     // public async Task GetSprite_CachesWithoutWatchingForChangesInProduction()
//     // { }
// }
