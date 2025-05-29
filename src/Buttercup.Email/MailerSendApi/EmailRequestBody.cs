namespace Buttercup.Email.MailerSendApi;

internal sealed record EmailRequestBody(
    EmailAddress From,
    EmailAddress[] To,
    string Subject,
    string Text);
