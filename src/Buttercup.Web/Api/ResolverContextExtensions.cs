using HotChocolate.Resolvers;

namespace Buttercup.Web.Api;

/// <summary>
/// Provides extension methods for <see cref="IResolverContext" />.
/// </summary>
public static class ResolverContextExtensions
{
    /// <summary>
    /// Creates a GraphQL execution error.
    /// </summary>
    /// <param name="resolverContext">The resolver context.</param>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The error.</returns>
    public static IError CreateError(
        this IResolverContext resolverContext, string code, string message) =>
        ErrorBuilder
            .New()
            .SetCode(code)
            .SetMessage(message)
            .SetPath(resolverContext.Path)
            .AddLocation(resolverContext.Selection.SyntaxNode)
            .Build();
}

