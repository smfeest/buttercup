using System.Data.Common;
using MySqlConnector;

namespace Buttercup.DataAccess;

/// <summary>
/// Provides extension methods for <see cref="DbCommand" />.
/// </summary>
internal static class MySqlParameterCollectionExtensions
{
    /// <summary>
    /// Appends a new parameter with a string value.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="MySqlParameterCollection.AddWithValue"/>, this method trims whitespace
    /// from the start and end of <paramref name="value"/> and treats strings containing only
    /// whitespace as null.
    /// </remarks>
    /// <param name="parameters">
    /// The parameter collection.
    /// </param>
    /// <param name="name">
    /// The parameter name.
    /// </param>
    /// <param name="value">
    /// The parameter value.
    /// </param>
    /// <returns>
    /// The new parameter.
    /// </returns>
    public static DbParameter AddWithStringValue(
        this MySqlParameterCollection parameters, string name, string? value)
    {
        value = string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        return parameters.AddWithValue(name, value);
    }
}
