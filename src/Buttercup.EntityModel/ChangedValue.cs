namespace Buttercup.EntityModel;

/// <summary>
/// Represents a changed value in an audit entry.
/// </summary>
/// <typeparam name="T">The value type.</typeparam>
public sealed class ChangedValue<T>
{
    /// <summary>
    /// Initializes a new instance representing a value on a new record.
    /// </summary>
    /// <param name="newValue">
    /// The new value.
    /// </param>
    public ChangedValue(T? newValue)
    {
        this.HasPreviousValue = false;
        this.NewValue = newValue;
    }

    /// <summary>
    /// Initializes a new instance representing a value on a modified record.
    /// </summary>
    /// <param name="previousValue">
    /// The previous value.
    /// </param>
    /// <param name="newValue">
    /// The new value.
    /// </param>
    public ChangedValue(T? previousValue, T? newValue)
    {
        this.HasPreviousValue = true;
        this.PreviousValue = previousValue;
        this.NewValue = newValue;
    }

    /// <summary>
    /// <b>true</b> if this object represents a change to an existing record; otherwise,
    /// <b>false</b>.
    /// </summary>
    public bool HasPreviousValue { get; } // TODO: Consider renaming to IsModification or similar?

    /// <summary>
    /// Gets the value of the property before the change.
    /// </summary>
    public T? PreviousValue { get; }

    /// <summary>
    /// Gets the value of the property after the change.
    /// </summary>
    public T? NewValue { get; }
}
