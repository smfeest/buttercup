using Xunit;

namespace Buttercup.Security;

public sealed class InvalidTokenExceptionTests
{
    [Fact]
    public void Constructor_WithoutArguments()
    {
        var ex = new InvalidTokenException();
        Assert.NotNull(ex.Message);
    }

    [Fact]
    public void Constructor_WithMessage()
    {
        var ex = new InvalidTokenException("Token has expired");
        Assert.Equal("Token has expired", ex.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException()
    {
        var inner = new InvalidOperationException();
        var ex = new InvalidTokenException("Token has expired", inner);
        Assert.Equal("Token has expired", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
}
