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
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Verifies that the deserialized value of a <see cref="JsonElement" /> is equivalent to a
    /// specified value.
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
    public static void Equivalent<TValue>(TValue expected, JsonElement element)
    {
        var actual = element.Deserialize<TValue>(JsonSerializerOptions);
        Assert.Equivalent(expected, actual, true);
    }

    /// <summary>
    /// Verifies that the value of a JSON element is null.
    /// </summary>
    /// <param name="element">
    /// The JSON element.
    /// </param>
    /// <exception cref="EqualException">
    /// The value of <paramref name="element" /> is not null.
    /// </exception>
    public static void ValueIsNull(JsonElement element) =>
        Assert.Equal(JsonValueKind.Null, element.ValueKind);
}
