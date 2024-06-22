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
    /// Verifies that a response has an errors field containing a single error, with a specific
    /// error code.
    /// </summary>
    /// <param name="expectedErrorCode">
    /// The expected error code.
    /// </param>
    /// <param name="document">
    /// The response document.
    /// </param>
    /// <returns>
    /// The error object as a <see cref="JsonElement" />.
    /// </returns>
    public static JsonElement HasSingleError(string expectedErrorCode, JsonDocument document)
    {
        Assert.True(document.RootElement.TryGetProperty("errors", out var errorsElement));

        var errorElement = errorsElement.EnumerateArray().Single();

        var actualErrorCode = errorElement
            .GetProperty("extensions")
            .GetProperty("code")
            .GetString();

        Assert.Equal(expectedErrorCode, actualErrorCode);

        return errorElement;
    }

    /// <summary>
    /// Verifies that a response has an errors field containing a single error, with a specific
    /// error code and message.
    /// </summary>
    /// <param name="expectedErrorCode">
    /// The expected error code.
    /// </param>
    /// <param name="expectedErrorMessage">
    /// The expected error message.
    /// </param>
    /// <param name="document">
    /// The response document.
    /// </param>
    /// <returns>
    /// The error object as a <see cref="JsonElement" />.
    /// </returns>
    public static JsonElement HasSingleError(
        string expectedErrorCode, string expectedErrorMessage, JsonDocument document)
    {
        var errorElement = HasSingleError(expectedErrorCode, document);
        Assert.Equal(expectedErrorMessage, errorElement.GetProperty("message").GetString());
        return errorElement;
    }

    /// <summary>
    /// Verifies that a response has a data field and no errors field.
    /// </summary>
    /// <param name="document">
    /// The response document.
    /// </param>
    /// <returns>
    /// The data field as a <see cref="JsonElement" />.
    /// </returns>
    public static JsonElement SuccessResponse(JsonDocument document)
    {
        Assert.True(document.RootElement.TryGetProperty("data", out var dataElement));
        Assert.False(document.RootElement.TryGetProperty("errors", out _));

        return dataElement;
    }
}
