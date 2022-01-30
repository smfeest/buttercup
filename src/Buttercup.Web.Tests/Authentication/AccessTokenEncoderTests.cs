using Microsoft.AspNetCore.DataProtection;
using Moq;
using Xunit;

namespace Buttercup.Web.Authentication;

public class AccessTokenEncoderTests
{
    public AccessTokenEncoderTests()
    {
        var dataProtectionProvider = Mock.Of<IDataProtectionProvider>(
            x => x.CreateProtector(nameof(Authentication.AccessTokenEncoder)) ==
                this.MockDataProtector.Object);

        this.AccessTokenEncoder = new(
            this.MockAccessTokenSerializer.Object,
            dataProtectionProvider);
    }

    private AccessTokenPayload Payload { get; } = new(1, "security-stamp", DateTime.UtcNow);

    private byte[] PayloadBytes { get; } = { 1, 2, 3 };

    private byte[] EncryptedBytes { get; } = { 4, 5, 6 };

    private string EncryptedBase64 { get; } = "BAUG";

    private Mock<IAccessTokenSerializer> MockAccessTokenSerializer { get; } = new();

    private Mock<IDataProtector> MockDataProtector { get; } = new();

    private AccessTokenEncoder AccessTokenEncoder { get; }

    #region Encode

    [Fact]
    public void EncodeReturnsEncodedPayload()
    {
        this.MockAccessTokenSerializer
            .Setup(x => x.Serialize(this.Payload))
            .Returns(this.PayloadBytes);

        this.MockDataProtector
            .Setup(x => x.Protect(this.PayloadBytes))
            .Returns(this.EncryptedBytes);

        var token = this.AccessTokenEncoder.Encode(this.Payload);

        Assert.Equal(this.EncryptedBase64, token);
    }

    #endregion

    #region Decode

    [Fact]
    public void DecodeReturnsDecodedPayload()
    {
        this.MockDataProtector
            .Setup(x => x.Unprotect(this.EncryptedBytes))
            .Returns(this.PayloadBytes);

        this.MockAccessTokenSerializer
            .Setup(x => x.Deserialize(this.PayloadBytes))
            .Returns(this.Payload);

        var decodedPayload = this.AccessTokenEncoder.Decode(this.EncryptedBase64);

        Assert.Equal(this.Payload, decodedPayload);
    }

    #endregion
}
