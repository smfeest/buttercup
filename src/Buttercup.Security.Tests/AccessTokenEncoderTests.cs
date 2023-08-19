using Microsoft.AspNetCore.DataProtection;
using Moq;
using Xunit;

namespace Buttercup.Security;

public sealed class AccessTokenEncoderTests
{
    private readonly AccessTokenPayload payload = new(1, "security-stamp", DateTime.UtcNow);
    private readonly byte[] payloadBytes = { 1, 2, 3 };
    private readonly byte[] encryptedBytes = { 4, 5, 6 };
    private readonly string encryptedBase64 = "BAUG";

    private readonly Mock<IAccessTokenSerializer> accessTokenSerializerMock = new();
    private readonly Mock<IDataProtector> dataProtectorMock = new();

    private readonly AccessTokenEncoder accessTokenEncoder;

    public AccessTokenEncoderTests()
    {
        var dataProtectionProvider = Mock.Of<IDataProtectionProvider>(
            x => x.CreateProtector(nameof(AccessTokenEncoder)) == this.dataProtectorMock.Object);

        this.accessTokenEncoder = new(
            this.accessTokenSerializerMock.Object,
            dataProtectionProvider);
    }

    #region Encode

    [Fact]
    public void EncodeReturnsEncodedPayload()
    {
        this.accessTokenSerializerMock
            .Setup(x => x.Serialize(this.payload))
            .Returns(this.payloadBytes);

        this.dataProtectorMock
            .Setup(x => x.Protect(this.payloadBytes))
            .Returns(this.encryptedBytes);

        var token = this.accessTokenEncoder.Encode(this.payload);

        Assert.Equal(this.encryptedBase64, token);
    }

    #endregion

    #region Decode

    [Fact]
    public void DecodeReturnsDecodedPayload()
    {
        this.dataProtectorMock
            .Setup(x => x.Unprotect(this.encryptedBytes))
            .Returns(this.payloadBytes);

        this.accessTokenSerializerMock
            .Setup(x => x.Deserialize(this.payloadBytes))
            .Returns(this.payload);

        var decodedPayload = this.accessTokenEncoder.Decode(this.encryptedBase64);

        Assert.Equal(this.payload, decodedPayload);
    }

    #endregion
}
