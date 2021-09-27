using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Xunit;

namespace Buttercup.Web.Infrastructure
{
    public class AssetHelperTests
    {
        #region Url

        [Theory]
        [InlineData("Development", "/alpha/beta/assets/gamma/delta.png")]
        [InlineData("Production", "/alpha/beta/prod-assets/gamma/delta-82fb493637.png")]
        public void ResolvesUrls(string environment, string expectedContentPath)
        {
            var actionContext = new ActionContext();

            var mockManifestSource = new Mock<IAssetManifestSource>();
            mockManifestSource
                .SetupGet(x => x.ProductionManifest)
                .Returns(new Dictionary<string, string>
                {
                    { "gamma/delta.png", "gamma/delta-82fb493637.png" },
                });

            var mockHostingEnvironment = new Mock<IWebHostEnvironment>();
            mockHostingEnvironment.SetupGet(x => x.EnvironmentName).Returns(environment);

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper
                .Setup(x => x.Content(It.IsAny<string>()))
                .Returns(
                    (string path) => path.Replace("~", "/alpha/beta", StringComparison.Ordinal));

            var mockUrlHelperFactory = new Mock<IUrlHelperFactory>();
            mockUrlHelperFactory
                .Setup(x => x.GetUrlHelper(actionContext))
                .Returns(mockUrlHelper.Object);

            var assetHelper = new AssetHelper(
                mockManifestSource.Object,
                mockHostingEnvironment.Object,
                mockUrlHelperFactory.Object);

            Assert.Equal(expectedContentPath, assetHelper.Url(actionContext, "gamma/delta.png"));
        }

        #endregion
    }
}
