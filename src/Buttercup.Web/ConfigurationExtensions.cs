namespace Buttercup.Web;

/// <summary>
/// Provides extension methods for <see cref="IConfiguration" />.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Gets a connection string
    /// </summary>
    /// <param name="configuration">
    /// The configuration to enumerate.
    /// </param>
    /// <param name="name">
    /// The connection string name.
    /// </param>
    /// <returns>
    /// The connection string.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// No connection string exists with the specified name.
    /// </exception>
    public static string GetRequiredConnectionString(this IConfiguration configuration, string name)
    {
        var connectionString = configuration.GetConnectionString(name);

        return string.IsNullOrEmpty(connectionString) ?
            throw new InvalidOperationException(
                $"ConnectionStrings section does not contain key '{name}'") :
            connectionString;
    }
}
