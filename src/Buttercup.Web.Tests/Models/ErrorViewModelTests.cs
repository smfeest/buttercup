using Xunit;

namespace Buttercup.Web.Models;

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
