using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// Provides extension methods for <see cref="DbCommand" />.
    /// </summary>
    internal static class DbCommandExtensions
    {
        /// <summary>
        /// Appends a new parameter to the command with a name and value.
        /// </summary>
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
        public static DbParameter AddParameterWithValue(
            this DbCommand command, string name, object? value)
        {
            var parameter = command.CreateParameter();

            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;

            command.Parameters.Add(parameter);

            return parameter;
        }

        /// <summary>
        /// Appends a new parameter to the command with a name and string value.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="AddParameterWithValue"/>, this method trims whitespace from the start
        /// and end of <paramref name="value"/> and treats strings containing only white space as
        /// null.
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
            this DbCommand command, string name, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                value = null;
            }
            else
            {
                value = value.Trim();
            }

            return command.AddParameterWithValue(name, value);
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

            if (rawValue == null || Convert.IsDBNull(rawValue))
            {
                return default(T);
            }

            return (T)rawValue;
        }
    }
}
