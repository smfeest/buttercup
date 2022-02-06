using Xunit;

namespace Buttercup.Web.Authentication;

public class AccessTokenSerializerTests
{
    [Fact]
    public void RoundTripsAllProperties()
    {
        var originalPayload = new AccessTokenPayload(4567890123, "security-stamp", DateTime.UtcNow);

        var serializer = new AccessTokenSerializer();

        var roundtrippedPayload = serializer.Deserialize(serializer.Serialize(originalPayload));

        Assert.Equal(originalPayload, roundtrippedPayload);
    }
}
