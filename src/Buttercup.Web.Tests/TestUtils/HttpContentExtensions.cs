using System.Text.Json;

namespace Buttercup.Web.TestUtils;

/// <summary>
/// Provides extension methods for <see cref="HttpContent" />.
/// </summary>
public static class HttpContentExtensions
{
    /// <summary>
    /// Parses the entity body represented by a <see cref="HttpContent" /> instance into a <see
    /// cref="JsonDocument" />.
    /// </summary>
    /// <param name="content">
    /// The content to read from.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is the <see cref="JsonDocument" />.
    /// </returns>
    public static async Task<JsonDocument> ReadAsJsonDocument(this HttpContent content)
    {
        using var stream = await content.ReadAsStreamAsync();

        return await JsonDocument.ParseAsync(stream);
    }
}
