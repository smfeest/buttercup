namespace Buttercup.Web.TestUtils;

/// <summary>
/// Provides extension methods for <see cref="HttpClient" />.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Posts a GraphQL query.
    /// </summary>
    /// <param name="client">
    /// The client used to send the request.
    /// </param>
    /// <param name="query">
    /// The query.
    /// </param>
    /// <param name="variables">
    /// The query variables.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is the HTTP response.
    /// </returns>
    public static Task<HttpResponseMessage> PostQuery(
        this HttpClient client, string query, object? variables = null) =>
        client.PostAsJsonAsync("/graphql", new { Query = query, Variables = variables ?? new() });
}
