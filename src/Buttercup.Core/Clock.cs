namespace Buttercup;

/// <summary>
/// The default implementation of <see cref="IClock" />.
/// </summary>
internal class Clock : IClock
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
