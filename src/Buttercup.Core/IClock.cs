namespace Buttercup
{
    /// <summary>
    /// Defines the contract for the clock service.
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// Gets the time now in Coordinated Universal Time (UTC).
        /// </summary>
        /// <value>
        /// The time now in Coordinated Universal Time (UTC).
        /// </value>
        DateTime UtcNow { get; }
    }
}
