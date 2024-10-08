using Microsoft.Extensions.Options;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using Xunit;

namespace Buttercup.Email;

public sealed class EmailSenderTests
{
    private const string FromAddress = "from@example.com";
    private const string ToAddress = "to@example.com";
    private const string Subject = "message-subject";
    private const string Body = "message-body";

    #region Send

    [Fact]
    public async Task Send_SetsFromAddress()
    {
        var message = await Send();

        Assert.Equal(FromAddress, message.From.Email);
    }

    [Fact]
    public async Task Send_SetsToAddress()
    {
        var message = await Send();

        var personalization = Assert.Single(message.Personalizations);
        var to = Assert.Single(personalization.Tos);
        Assert.Equal(ToAddress, to.Email);
    }

    [Fact]
    public async Task Send_SetsSubject()
    {
        var message = await Send();

        Assert.Equal(Subject, message.Subject);
    }

    [Fact]
    public async Task Send_SetsBody()
    {
        var message = await Send();

        Assert.Equal(Body, message.PlainTextContent);
    }

    private static async Task<SendGridMessage> Send()
    {
        var mockClient = new Mock<ISendGridClient>();
        var clientAccessor = Mock.Of<ISendGridClientAccessor>(
            x => x.SendGridClient == mockClient.Object);
        var options = Options.Create(
            new EmailOptions { ApiKey = string.Empty, FromAddress = FromAddress });
        var emailSender = new EmailSender(clientAccessor, options);

        SendGridMessage? sentMessage = null;

        mockClient
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), CancellationToken.None))
            .Callback<SendGridMessage, CancellationToken>((message, _) => sentMessage = message);

        await emailSender.Send(ToAddress, Subject, Body);

        return sentMessage!;
    }

    #endregion
}
