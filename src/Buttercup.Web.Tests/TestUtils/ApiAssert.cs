using System.Net;
using System.Text.Json;
using Xunit;

namespace Buttercup.Web.TestUtils;

/// <summary>
/// Contains static methods that can be used to verify that conditions are satisfied by API
/// responses.
/// </summary>
public static class ApiAssert
{
    /// <summary>
    /// Verifies that a response has the <see cref="HttpStatusCode.OK" /> status code, a data field,
    /// and no error field.
    /// </summary>
    /// <param name="response">
    /// The response.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is the data field as a <see cref="JsonElement" />.
    /// </returns>
    public static async Task<JsonElement> SuccessResponse(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var document = await response.Content.ReadAsJsonDocument();

        Assert.True(document.RootElement.TryGetProperty("data", out var dataElement));
        Assert.False(document.RootElement.TryGetProperty("errors", out _));

        return dataElement;
    }
}
