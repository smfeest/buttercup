namespace Buttercup.Web.Api;

/// <summary>
/// Represents the error raised when an authentication attempt is blocked due to too many failed
/// attempts.
/// </summary>
/// <param name="Message">
/// The error message.
/// </param>
public record TooManyAttemptsError(string Message) : AuthenticateError(Message);
