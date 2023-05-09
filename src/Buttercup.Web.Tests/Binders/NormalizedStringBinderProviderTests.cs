using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Moq;
using Xunit;

namespace Buttercup.Web.Binders;

public class NormalizedStringBinderProviderTests
{
    #region GetBinder

    [Fact]
    public void GetBinderReturnsNormalizedStringBinderWhenModelTypeIsString()
    {
        var provider = new NormalizedStringBinderProvider();
        var context = CreateContext<string>();

        Assert.IsType<NormalizedStringBinder>(provider.GetBinder(context));
    }

    [Fact]
    public void GetBinderReturnsNullWhenModelTypeIsNotString()
    {
        var provider = new NormalizedStringBinderProvider();
        var context = CreateContext<int>();

        Assert.Null(provider.GetBinder(context));
    }

    #endregion

    private static ModelBinderProviderContext CreateContext<T>()
    {
        var metadataIdentity = ModelMetadataIdentity.ForType(typeof(T));
        var metadata = new Mock<ModelMetadata>(metadataIdentity).Object;

        return Mock.Of<ModelBinderProviderContext>(x => x.Metadata == metadata);
    }
}
