namespace Buttercup;

/// <summary>
/// Provides extension methods for <see cref="TimeProvider" />.
/// </summary>
public static class TimeProviderExtensions
{
    /// <summary>
    /// Gets the <see cref="DateTime"/> now in Coordinated Universal Time (UTC).
    /// </summary>
    /// <param name="timeProvider">
    /// The time provider.
    /// </param>
    /// <returns>
    /// The <see cref="DateTime"/> now in Coordinated Universal Time (UTC).
    /// </returns>
    public static DateTime GetUtcDateTimeNow(this TimeProvider timeProvider) =>
        timeProvider.GetUtcNow().UtcDateTime;
}
