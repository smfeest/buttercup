using Xunit;

namespace Buttercup.EntityModel;

public sealed class UserAuditEntryTests
{
    #region IsSuccess

    [Fact]
    public void IsSuccess_ReturnsTrueIfFailureIsNull()
    {
        var entry = new UserAuditEntry { Failure = null };
        Assert.True(entry.IsSuccess);
    }

    [Fact]
    public void IsSuccess_ReturnsFalseIfFailureIsNotNull()
    {
        var entry = new UserAuditEntry { Failure = UserAuditFailure.IncorrectPassword };
        Assert.False(entry.IsSuccess);
    }

    #endregion
}
