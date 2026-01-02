using Xunit;

namespace Buttercup.Security;

public sealed class ParameterMaskingServiceTests
{
    #region MaskToken

    [Theory]
    [InlineData("", "")]
    [InlineData("abcde", "abcde")]
    [InlineData("abcdef", "abcdef")]
    [InlineData("abcdefg", "abcdef…")]
    [InlineData("abcdefghi", "abcdef…")]
    public void MaskToken_RevealsFirstSixCharacters(string token, string expectedMaskedToken)
    {
        var maskedToken = new ParameterMaskingService().MaskToken(token);
        Assert.Equal(expectedMaskedToken, maskedToken);
    }

    #endregion
}
