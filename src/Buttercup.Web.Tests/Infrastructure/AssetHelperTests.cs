using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Xunit;

namespace Buttercup.Web.Infrastructure;

public class AssetHelperTests
{
    #region Url

    [Theory]
    [InlineData("Development", "/alpha/beta/assets/gamma/delta.png")]
    [InlineData("Production", "/alpha/beta/prod-assets/gamma/delta-82fb493637.png")]
    public void ResolvesUrls(string environment, string expectedContentPath)
    {
        var actionContext = new ActionContext();

        var manifestSource = Mock.Of<IAssetManifestSource>(
            x => x.ProductionManifest == new Dictionary<string, string>
            {
                { "gamma/delta.png", "gamma/delta-82fb493637.png" },
            });

        var hostingEnvironment = Mock.Of<IWebHostEnvironment>(
            x => x.EnvironmentName == environment);

        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper
            .Setup(x => x.Content(It.IsAny<string>()))
            .Returns((string path) => path.Replace("~", "/alpha/beta", StringComparison.Ordinal));

        var urlHelperFactory = Mock.Of<IUrlHelperFactory>(
            x => x.GetUrlHelper(actionContext) == mockUrlHelper.Object);

        var assetHelper = new AssetHelper(manifestSource, hostingEnvironment, urlHelperFactory);

        Assert.Equal(expectedContentPath, assetHelper.Url(actionContext, "gamma/delta.png"));
    }

    #endregion
}
