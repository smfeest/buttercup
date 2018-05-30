using System;
using System.Data;
using System.Data.Common;

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
        public static DbParameter AddParameterWithValue(
            this DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();

            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;

            command.Parameters.Add(parameter);

            return parameter;
        }
    }
}
