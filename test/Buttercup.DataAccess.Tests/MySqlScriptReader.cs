using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// Represents a reader that can read statements from a MySQL script.
    /// </summary>
    /// <remarks>
    /// This reader supports custom statement delimiters through the MySQL `delimiter` command.
    /// </remarks>
    public class MySqlScriptReader
    {
        private string currentDelimiter = ";";

        public MySqlScriptReader(TextReader reader) => this.Reader = reader;

        /// <summary>
        /// Gets the text reader.
        /// </summary>
        /// <value>
        /// The text reader.
        /// </value>
        public TextReader Reader { get; }

        /// <summary>
        /// Reads a statement from the script.
        /// </summary>
        /// <returns>
        /// A task for the operation. The value is the statement, or a null reference if all of the
        /// statements have been read.
        /// </returns>
        public async Task<string?> ReadStatement()
        {
            var builder = new StringBuilder();

            string? line;

            while ((line = await this.Reader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("DELIMITER ", StringComparison.OrdinalIgnoreCase))
                {
                    this.currentDelimiter = line.Substring(10).Trim();
                }
                else if (line.EndsWith(this.currentDelimiter, StringComparison.Ordinal))
                {
                    builder.AppendLine(
                        line.Substring(0, line.Length - this.currentDelimiter.Length));
                    break;
                }
                else
                {
                    builder.AppendLine(line);
                }
            }

            var command = builder.ToString();

            if (string.IsNullOrWhiteSpace(command))
            {
                command = null;
            }

            return command;
        }
    }
}
