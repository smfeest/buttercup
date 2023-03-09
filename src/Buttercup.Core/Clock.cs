namespace Buttercup;

internal class Clock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
