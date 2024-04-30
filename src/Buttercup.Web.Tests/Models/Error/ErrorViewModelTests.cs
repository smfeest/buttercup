using Xunit;

#pragma warning disable CA1716
namespace Buttercup.Web.Models.Error;
#pragma warning restore CA1716

public sealed class ErrorViewModelTests
{
    #region ShowRequestId

    [Theory]
    [InlineData("", false)]
    [InlineData("ABC123", true)]
    public void ShowRequestId_ReturnsTrueIfRequestIdIsSpecified(
        string requestId, bool expectedResult) =>
        Assert.Equal(expectedResult, new ErrorViewModel(requestId).ShowRequestId);

    #endregion
}
