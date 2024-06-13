using HotChocolate.Resolvers;

namespace Buttercup.Web.Api;

/// <summary>
/// Provides methods for creating GraphQL execution errors.
/// </summary>
public static class ErrorFactory
{
    /// <summary>
    /// Creates a GraphQL authorization error.
    /// </summary>
    /// <param name="resolverContext">The resolver context.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The GraphQL authorization error.</returns>
    public static IError AuthorizationError(IResolverContext resolverContext, string message) =>
        ErrorBuilder
            .New()
            .SetMessage(message)
            .SetCode(ErrorCodes.Authentication.NotAuthorized)
            .SetPath(resolverContext.Path)
            .AddLocation(resolverContext.Selection.SyntaxNode)
            .Build();
}

