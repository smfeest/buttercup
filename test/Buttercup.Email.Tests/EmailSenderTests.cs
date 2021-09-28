using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using Xunit;

namespace Buttercup.Email
{
    public class EmailSenderTests
    {
        #region Send

        [Fact]
        public async Task SendSetsFromAddress()
        {
            var context = new Context(new() { FromAddress = "buttercup@example.com" });

            await context.EmailSender.Send(
                "sample.recipient@example.com", "Sample subject", "Sample body");

            Assert.Equal("buttercup@example.com", context.SentMessage.From.Email);
        }

        [Fact]
        public async Task SendSetsToAddress()
        {
            var context = new Context();

            await context.EmailSender.Send(
                "sample.recipient@example.com", "Sample subject", "Sample body");

            var personalization = Assert.Single(context.SentMessage.Personalizations);
            var to = Assert.Single(personalization.Tos);
            Assert.Equal("sample.recipient@example.com", to.Email);
        }

        [Fact]
        public async Task SendSetsSubject()
        {
            var context = new Context();

            await context.EmailSender.Send(
                "sample.recipient@example.com", "Sample subject", "Sample body");

            Assert.Equal("Sample subject", context.SentMessage.Subject);
        }

        [Fact]
        public async Task SendSetsBody()
        {
            var context = new Context();

            await context.EmailSender.Send(
                "sample.recipient@example.com", "Sample subject", "Sample body");

            Assert.Equal("Sample body", context.SentMessage.PlainTextContent);
        }

        #endregion

        private class Context
        {
            public Context(EmailOptions emailOptions = null)
            {
                this.EmailSender = new(
                    new FakeSendGridClientAccessor(this.MockSendGridClient.Object),
                    Options.Create(emailOptions ?? new()));

                this.MockSendGridClient
                    .Setup(
                        x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), CancellationToken.None))
                    .Returns((SendGridMessage message, CancellationToken cancellationToken) =>
                    {
                        this.SentMessage = message;

                        return Task.FromResult<Response>(null);
                    });
            }

            public EmailSender EmailSender { get; }

            public Mock<ISendGridClient> MockSendGridClient { get; } = new();

            public SendGridMessage SentMessage { get; private set; }
        }

        private class FakeSendGridClientAccessor : ISendGridClientAccessor
        {
            public FakeSendGridClientAccessor(ISendGridClient sendGridClient) =>
                this.SendGridClient = sendGridClient;

            public ISendGridClient SendGridClient { get; }
        }
    }
}
