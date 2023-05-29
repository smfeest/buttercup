using System.Text.Json;
using Xunit;
using Xunit.Sdk;

namespace Buttercup.Web.TestUtils;

/// <summary>
/// Contains static methods that can be used to verify that conditions are satisfied by JSON
/// elements.
/// </summary>
public static class JsonAssert
{
    /// <summary>
    /// Verifies that the deserialized value of a <see cref="JsonElement" /> is equal to a specified
    /// value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Properties in <paramref name="element" /> are assumed to be camel-cased.
    /// </para>
    /// <para>
    /// Properties that exist in <paramref name="element" /> but not in <paramref name="expected" />
    /// are ignored.
    /// </para>
    /// </remarks>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="expected">
    /// The expected deserialized value of <paramref name="element" />.
    /// </param>
    /// <param name="element">The <see cref="JsonElement" /> to be inspected.</param>
    /// <exception cref="EqualException">
    /// When the deserialized value of <paramref name="element" /> is not equivalent to <paramref
    /// name="expected" />
    /// </exception>
    public static void ValueEquals<TValue>(TValue expected, JsonElement element)
    {
        var actual = element.Deserialize<TValue>(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        Assert.Equal(expected, actual);
    }
}
