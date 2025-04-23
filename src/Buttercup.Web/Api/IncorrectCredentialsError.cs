namespace Buttercup.Web.Api;

/// <summary>
/// Represents the error raised when an authentication attempt fails due to incorrect credentials.
/// </summary>
/// <param name="Message">
/// The error message.
/// </param>
public record IncorrectCredentialsError(string Message) : AuthenticateError(Message);
