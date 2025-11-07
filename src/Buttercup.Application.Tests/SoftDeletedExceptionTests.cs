using Xunit;

namespace Buttercup.Application;

public sealed class SoftDeletedExceptionTests
{
    [Fact]
    public void Constructor_WithoutArguments()
    {
        var ex = new SoftDeletedException();
        Assert.NotNull(ex.Message);
    }

    [Fact]
    public void Constructor_WithMessage()
    {
        var ex = new SoftDeletedException("Record is soft-deleted");
        Assert.Equal("Record is soft-deleted", ex.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException()
    {
        var inner = new InvalidOperationException();
        var ex = new SoftDeletedException("Record is soft-deleted", inner);
        Assert.Equal("Record is soft-deleted", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
}
