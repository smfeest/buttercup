using System;
using System.Data.Common;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// Provides extension methods for <see cref="DbDataReader" />.
    /// </summary>
    internal static class DbDataReaderExtensions
    {
        private delegate T ReadValue<T>(int ordinal);

        /// <summary>
        /// Gets the value in a column as a <see cref="DateTime" /> value.
        /// </summary>
        /// <param name="reader">
        /// The data reader.
        /// </param>
        /// <param name="columnName">
        /// The column name.
        /// </param>
        /// <param name="kind">
        /// A value that indicates whether the value represents a local or UTC date and time, or
        /// neither.
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
        public static DateTime GetDateTime(
            this DbDataReader reader, string columnName, DateTimeKind kind) =>
            reader.GetValue(
                i => DateTime.SpecifyKind(reader.GetDateTime(i), kind), columnName, false);

        /// <summary>
        /// Gets the value in a column as a 32-bit signed integer.
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
        public static int GetInt32(this DbDataReader reader, string columnName) =>
            reader.GetValue(reader.GetInt32, columnName, false);

        /// <summary>
        /// Gets the value in a column as a 64-bit signed integer.
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
        public static long GetInt64(this DbDataReader reader, string columnName) =>
            reader.GetValue(reader.GetInt64, columnName, false);

        /// <summary>
        /// Gets the value in a column as a nullable <see cref="DateTime" /> value.
        /// </summary>
        /// <param name="reader">
        /// The data reader.
        /// </param>
        /// <param name="columnName">
        /// The column name.
        /// </param>
        /// <param name="kind">
        /// A value that indicates whether the value represents a local or UTC date and time, or
        /// neither.
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
            this DbDataReader reader, string columnName, DateTimeKind kind) =>
            reader.GetValue<DateTime?>(
                ordinal => DateTime.SpecifyKind(reader.GetDateTime(ordinal), kind),
                columnName,
                true);

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
        public static int? GetNullableInt32(this DbDataReader reader, string columnName) =>
            reader.GetValue<int?>(ordinal => reader.GetInt32(ordinal), columnName, true);

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
        public static long? GetNullableInt64(this DbDataReader reader, string columnName) =>
            reader.GetValue<long?>(ordinal => reader.GetInt64(ordinal), columnName, true);

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
        public static string GetString(this DbDataReader reader, string columnName) =>
            reader.GetValue(reader.GetString, columnName, true);

        private static T GetValue<T>(
            this DbDataReader reader, ReadValue<T> readValue, string columnName, bool canBeNull)
        {
            var ordinal = reader.GetOrdinal(columnName);

            if (canBeNull && reader.IsDBNull(ordinal))
            {
                return default(T);
            }

            return readValue(ordinal);
        }
    }
}
