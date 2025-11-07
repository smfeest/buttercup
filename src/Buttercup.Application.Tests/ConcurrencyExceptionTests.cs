using Xunit;

namespace Buttercup.Application;

public sealed class ConcurrencyExceptionTests
{
    [Fact]
    public void Constructor_WithoutArguments()
    {
        var ex = new ConcurrencyException();
        Assert.NotNull(ex.Message);
    }

    [Fact]
    public void Constructor_WithMessage()
    {
        var ex = new ConcurrencyException("Save aborted due to conflicting changes");
        Assert.Equal("Save aborted due to conflicting changes", ex.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException()
    {
        var inner = new InvalidOperationException();
        var ex = new ConcurrencyException("Save aborted due to conflicting changes", inner);
        Assert.Equal("Save aborted due to conflicting changes", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
}
