namespace Buttercup.Web.Api;

/// <summary>
/// Represents the error raised when an invalid value is provided for an input field.
/// </summary>
/// <param name="Message">
/// The error message.
/// </param>
/// <param name="Path">
/// The field path.
/// </param>
/// <param name="Code">
/// The validation error code.
/// </param>
[GraphQLName("ValidationError")]
public record InputObjectValidationError(string Message, string[] Path, ValidationErrorCode Code);
