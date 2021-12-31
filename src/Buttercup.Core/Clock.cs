namespace Buttercup;

/// <summary>
/// The default implementation of <see cref="IClock" />.
/// </summary>
public class Clock : IClock
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
