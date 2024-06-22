using HotChocolate.Execution.Processing;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using Moq;
using Xunit;
using Location = HotChocolate.Location;
using Path = HotChocolate.Path;

namespace Buttercup.Web.Api;

public sealed class ResolverContextExtensionsTests
{
    #region CreateError

    [Fact]
    public void CreateError_ReturnsErrorWithMessageCodePathAndLocation()
    {
        const string Code = "FAKE_ERROR";
        const string Message = "Let's pretend something went wrong";

        var path = Path.Root.Append("foo");
        var selection = Mock.Of<ISelection>(
            x => x.SyntaxNode == new FieldNode("bar").WithLocation(new(1, 2, 3, 4)));
        var resolverContext = Mock.Of<IResolverContext>(
            x => x.Path == path && x.Selection == selection);

        var error = resolverContext.CreateError(Code, Message);

        Assert.Equal(Code, error.Code);
        Assert.Equal(Message, error.Message);
        Assert.Equal([new Location(3, 4)], error.Locations);
        Assert.Equal(path, error.Path);
    }

    #endregion
}
