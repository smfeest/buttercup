using System;
using MySqlConnector;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// Provides extension methods for <see cref="MySqlDataReader" />.
    /// </summary>
    internal static class MySqlDataReaderExtensions
    {
        /// <summary>
        /// Gets the value in a column as a nullable <see cref="DateTime" /> value.
        /// </summary>
        /// <param name="reader">
        /// The data reader.
        /// </param>
        /// <param name="columnName">
        /// The column name.
        /// </param>
        /// <returns>
        /// The value.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// No column with the specified name was found.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// The specified cast is not valid.
        /// </exception>
        public static DateTime? GetNullableDateTime(
            this MySqlDataReader reader, string columnName) =>
            reader.IsDBNull(columnName) ? null : reader.GetDateTime(columnName);

        /// <summary>
        /// Gets the value in a column as a nullable 32-bit signed integer.
        /// </summary>
        /// <param name="reader">
        /// The data reader.
        /// </param>
        /// <param name="columnName">
        /// The column name.
        /// </param>
        /// <returns>
        /// The value.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// No column with the specified name was found.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// The specified cast is not valid.
        /// </exception>
        public static int? GetNullableInt32(this MySqlDataReader reader, string columnName) =>
            reader.IsDBNull(columnName) ? null : reader.GetInt32(columnName);

        /// <summary>
        /// Gets the value in a column as a nullable 64-bit signed integer.
        /// </summary>
        /// <param name="reader">
        /// The data reader.
        /// </param>
        /// <param name="columnName">
        /// The column name.
        /// </param>
        /// <returns>
        /// The value.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// No column with the specified name was found.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// The specified cast is not valid.
        /// </exception>
        public static long? GetNullableInt64(this MySqlDataReader reader, string columnName) =>
            reader.IsDBNull(columnName) ? null : reader.GetInt64(columnName);

        /// <summary>
        /// Gets the value in a column as a string.
        /// </summary>
        /// <param name="reader">
        /// The data reader.
        /// </param>
        /// <param name="columnName">
        /// The column name.
        /// </param>
        /// <returns>
        /// The value.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// No column with the specified name was found.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// The specified cast is not valid.
        /// </exception>
        public static string? GetNullableString(this MySqlDataReader reader, string columnName) =>
            reader.IsDBNull(columnName) ? null : reader.GetString(columnName);

        private static bool IsDBNull(this MySqlDataReader reader, string columnName) =>
            reader.IsDBNull(reader.GetOrdinal(columnName));
    }
}
