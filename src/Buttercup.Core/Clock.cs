namespace Buttercup;

internal sealed class Clock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
