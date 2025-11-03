namespace Buttercup.Web.Api;

/// <summary>
/// Defines the error codes for validation errors.
/// </summary>
public enum ValidationErrorCode
{
    /// <summary>
    /// Indicates that the string value provided is not in a valid format for the field.
    /// </summary>
    InvalidFormat,
    /// <summary>
    /// Indicates that the string value provided is too long or too short for the field.
    /// </summary>
    InvalidStringLength,
    /// <summary>
    /// Indicates that the value provided is out of range for the field.
    /// </summary>
    OutOfRange,
    /// <summary>
    /// Indicates that the field requires a value and none was provided.
    /// </summary>
    Required,
}
