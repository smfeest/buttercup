using Microsoft.Extensions.Localization;
using Moq;

namespace Buttercup.Web.TestUtils;

internal static class MockLocalizerExtensions
{
    public static Mock<IStringLocalizer<T>> SetupLocalizedString<T>(
        this Mock<IStringLocalizer<T>> mockLocalizer, string name, string translatedString)
    {
        mockLocalizer
            .SetupGet(x => x[name])
            .Returns(new LocalizedString(string.Empty, translatedString));

        return mockLocalizer;
    }

    public static Mock<IStringLocalizer<T>> SetupLocalizedString<T>(
        this Mock<IStringLocalizer<T>> mockLocalizer,
        string name,
        object[] arguments,
        string translatedString)
    {
        mockLocalizer
            .SetupGet(x => x[name, arguments])
            .Returns(new LocalizedString(string.Empty, translatedString));

        return mockLocalizer;
    }
}
