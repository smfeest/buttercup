using System.ComponentModel.DataAnnotations;

namespace Buttercup.DataAccess;

/// <summary>
/// The data access options.
/// </summary>
public class DataAccessOptions
{
    /// <summary>
    /// Gets or sets the database connection string.
    /// </summary>
    /// <value>
    /// The database connection string.
    /// </value>
    [Required]
    public required string ConnectionString { get; set; }
}
