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
