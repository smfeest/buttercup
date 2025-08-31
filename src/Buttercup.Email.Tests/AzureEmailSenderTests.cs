using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Buttercup.Email;

public sealed class AzureEmailSenderTests
{
    private const string FromAddress = "from@example.com";
    private const string ToAddress = "to@example.com";
    private const string Subject = "message-subject";
    private const string Body = "message-body";

    #region Send

    [Fact]
    public async Task Send_SetsSenderAddress()
    {
        var message = await Send();

        Assert.Equal(FromAddress, message.SenderAddress);
    }

    [Fact]
    public async Task Send_SetsToAddress()
    {
        var message = await Send();

        var to = Assert.Single(message.Recipients.To);
        Assert.Equal(ToAddress, to.Address);
    }

    [Fact]
    public async Task Send_SetsSubject()
    {
        var message = await Send();

        Assert.Equal(Subject, message.Content.Subject);
    }

    [Fact]
    public async Task Send_SetsBody()
    {
        var message = await Send();

        Assert.Equal(Body, message.Content.PlainText);
    }

    private static async Task<EmailMessage> Send()
    {
        var emailClientMock = new Mock<EmailClient>();
        var options = Options.Create(
            new EmailOptions { FromAddress = FromAddress });
        var emailSender = new AzureEmailSender(emailClientMock.Object, options);

        EmailMessage? sentMessage = null;

        emailClientMock
            .Setup(x => x.SendAsync(WaitUntil.Started, It.IsAny<EmailMessage>(), default))
            .Callback<WaitUntil, EmailMessage, CancellationToken>(
                (_, message, _) => sentMessage = message);

        await emailSender.Send(ToAddress, Subject, Body);

        return sentMessage!;
    }

    #endregion
}
