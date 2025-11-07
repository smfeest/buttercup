using Xunit;

namespace Buttercup.EntityModel;

public sealed class NotFoundExceptionTests
{
    [Fact]
    public void Constructor_WithoutArguments()
    {
        var ex = new NotFoundException();
        Assert.NotNull(ex.Message);
    }

    [Fact]
    public void Constructor_WithMessage()
    {
        var ex = new NotFoundException("Record not found");
        Assert.Equal("Record not found", ex.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException()
    {
        var inner = new InvalidOperationException();
        var ex = new NotFoundException("Record not found", inner);
        Assert.Equal("Record not found", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
}
