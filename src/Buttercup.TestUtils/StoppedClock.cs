namespace Buttercup.TestUtils;

/// <summary>
/// A stopped clock that can be set to a specific time.
/// </summary>
public class StoppedClock : IClock
{
    /// <summary>
    /// Gets or sets the fake time now in Coordinated Universal Time (UTC).
    /// </summary>
    /// <value>
    /// The fake time now in Coordinated Universal Time (UTC).
    /// </value>
    public DateTime UtcNow { get; set; } = DateTime.UtcNow;
}
