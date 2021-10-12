using System;
using System.Data.Common;
using System.Threading.Tasks;
using MySqlConnector;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// Provides extension methods for <see cref="DbCommand" />.
    /// </summary>
    internal static class DbCommandExtensions
    {
        /// <summary>
        /// Appends a new parameter to the command with a name and string value.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="MySqlParameterCollection.AddWithValue"/>, this method trims white space
        /// from the start and end of <paramref name="value"/> and treats strings containing only
        /// whitespace as null.
        /// </remarks>
        /// <param name="command">
        /// The command.
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
        public static DbParameter AddParameterWithStringValue(
            this MySqlCommand command, string name, string? value)
        {
            value = string.IsNullOrWhiteSpace(value) ? null : value.Trim();

            return command.Parameters.AddWithValue(name, value);
        }

        /// <summary>
        /// Executes the command and gets the value in the first column of the first row in the
        /// result set.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="DbCommand.ExecuteScalarAsync()"/>, this method returns the same value
        /// when the result set is empty as it does when the value in the first column of the first
        /// row is <see cref="DBNull"/>.
        /// </remarks>
        /// <typeparam name="T">
        /// The data type for the first column.
        /// </typeparam>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <returns>
        /// A task for the operation. Result is the value in the first column of the first row in
        /// the result set, or the default value of <typeparamref name="T"/> if the result set is
        /// empty or the first column in the first row contains <see cref="DBNull"/>.
        /// </returns>
        public static async Task<T?> ExecuteScalarAsync<T>(this DbCommand command)
        {
            var rawValue = await command.ExecuteScalarAsync();

            return rawValue == null || Convert.IsDBNull(rawValue) ? default : (T)rawValue;
        }
    }
}
