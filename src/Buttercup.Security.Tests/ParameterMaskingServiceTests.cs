using Xunit;

namespace Buttercup.Security;

public sealed class ParameterMaskingServiceTests
{
    #region MaskEmail

    [Theory]
    [InlineData("", "")]
    [InlineData("a", "*")]
    [InlineData("abcde", "*****")]
    [InlineData("abcdef", "ab**ef")]
    [InlineData("abcdefgh", "ab****gh")]
    [InlineData("abcdef@", "ab**ef@")]
    [InlineData("abcdef@ghijk", "ab**ef@*****")]
    [InlineData("abcdef@ghijkl", "ab**ef@gh**kl")]
    [InlineData("abcdef@ghijklmn", "ab**ef@gh****mn")]
    [InlineData("abcdef@ghijkl@mn", "ab**ef@gh**kl@**")]
    public void MaskEmail_RevealsFirstAndLastTwoCharactersOfEachPart(
        string email, string expectedMaskedEmail)
    {
        var maskedEmail = new ParameterMaskingService().MaskEmail(email);
        Assert.Equal(expectedMaskedEmail, maskedEmail);
    }

    #endregion

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
