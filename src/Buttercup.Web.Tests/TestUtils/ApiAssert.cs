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
    /// Verifies that a response has a data field and no errors field.
    /// </summary>
    /// <param name="document">
    /// The response document.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is the data field as a <see cref="JsonElement" />.
    /// </returns>
    public static JsonElement SuccessResponse(JsonDocument document)
    {
        Assert.True(document.RootElement.TryGetProperty("data", out var dataElement));
        Assert.False(document.RootElement.TryGetProperty("errors", out _));

        return dataElement;
    }
}
