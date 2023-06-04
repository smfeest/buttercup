using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using Xunit;

namespace Buttercup.Web.Binders;

public sealed class NormalizedStringBinderTests
{
    private const string ModelName = "ExampleModelName";

    #region BindModelAsync

    [Fact]
    public async Task BindModelAsyncDoesNotSetStateOrModelWhenValueNotProvided()
    {
        var bindingContext = CreateBindingContext(ValueProviderResult.None);

        await new NormalizedStringBinder().BindModelAsync(bindingContext);

        Assert.False(bindingContext.ModelState.ContainsKey(ModelName));
        Assert.False(bindingContext.Result.IsModelSet);
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("  \t\n   ", null)]
    [InlineData("  \tRed\nGreen Blue\n   ", "Red\nGreen Blue")]
    public async Task BindModelAsyncSetsStateAndModelWhenValueProvided(
        string? providedValue, string? expectedModelValue)
    {
        var bindingContext = CreateBindingContext(new(providedValue));

        await new NormalizedStringBinder().BindModelAsync(bindingContext);

        var stateEntry = bindingContext.ModelState[ModelName];
        Assert.NotNull(stateEntry);
        Assert.Equal(providedValue, stateEntry.RawValue);

        var result = bindingContext.Result;
        Assert.True(result.IsModelSet);
        Assert.Equal(expectedModelValue, result.Model);
    }

    #endregion

    private static ModelBindingContext CreateBindingContext(ValueProviderResult valueProviderResult)
    {
        var mockBindingContext = new Mock<ModelBindingContext>();

        mockBindingContext.SetupGet(x => x.ModelName).Returns(ModelName);

        var modelState = new ModelStateDictionary();
        mockBindingContext.SetupGet(x => x.ModelState).Returns(modelState);

        mockBindingContext.SetupProperty(x => x.Result, ModelBindingResult.Failed());

        var valueProvider = Mock.Of<IValueProvider>(
            x => x.GetValue(ModelName) == valueProviderResult);
        mockBindingContext.SetupGet(x => x.ValueProvider).Returns(valueProvider);

        return mockBindingContext.Object;
    }
}
