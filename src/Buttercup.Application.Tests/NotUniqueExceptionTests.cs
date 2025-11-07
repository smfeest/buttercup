using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Buttercup.Application;

public sealed class NotUniqueExceptionTests
{
    [Fact]
    public void Constructor_WithPropertyName()
    {
        var ex = new NotUniqueException("Email");
        Assert.Equal("Email", ex.PropertyName);
    }

    [Fact]
    public void Constructor_WithPropertyNameAndMessage()
    {
        var ex = new NotUniqueException("Username", "Username is not unique");
        Assert.Equal("Username", ex.PropertyName);
        Assert.Equal("Username is not unique", ex.Message);
    }

    [Fact]
    public void Constructor_WithPropertyNameMessageAndInnerException()
    {
        var inner = new DbUpdateException();
        var ex = new NotUniqueException("Email", "Email is not unique", inner);
        Assert.Equal("Email", ex.PropertyName);
        Assert.Equal("Email is not unique", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
}
