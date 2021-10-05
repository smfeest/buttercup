using System;

namespace Buttercup
{
    /// <summary>
    /// Defines the contract for the clock service.
    /// </summary>
    public class Clock : IClock
    {
        /// <inheritdoc />
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
