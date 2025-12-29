using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Security;

public sealed class PasswordResetResultTests
{
    #region Failure

    [Fact]
    public void Failure_SuccessResult_ReturnsNull()
    {
        var result = new PasswordResetResult(new ModelFactory().BuildUser());
        Assert.Null(result.Failure);
    }

    [Fact]
    public void Failure_FailureResult_ReturnsFailure()
    {
        var result = new PasswordResetResult(PasswordResetFailure.InvalidToken);
        Assert.Equal(PasswordResetFailure.InvalidToken, result.Failure);
    }

    #endregion

    #region IsSuccess

    [Fact]
    public void IsSuccess_SuccessResult_ReturnsTrue()
    {
        var result = new PasswordResetResult(new ModelFactory().BuildUser());
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void IsSuccess_FailureResult_ReturnsFalse()
    {
        var result = new PasswordResetResult(PasswordResetFailure.InvalidToken);
        Assert.False(result.IsSuccess);
    }

    #endregion

    #region User

    [Fact]
    public void User_SuccessResult_ReturnsUser()
    {
        var user = new ModelFactory().BuildUser();
        var result = new PasswordResetResult(user);
        Assert.Equal(user, result.User);
    }

    [Fact]
    public void User_FailureResult_ReturnsNull()
    {
        var result = new PasswordResetResult(PasswordResetFailure.InvalidToken);
        Assert.Null(result.User);
    }

    #endregion
}
