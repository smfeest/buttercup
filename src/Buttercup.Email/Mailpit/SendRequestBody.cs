namespace Buttercup.Email.Mailpit;

internal sealed record SendRequestBody(
    EmailAddress From, EmailAddress[] To, string Subject, string Text);
