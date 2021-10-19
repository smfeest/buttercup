using Microsoft.Extensions.Localization;
using Moq;

namespace Buttercup.Web.TestUtils
{
    internal static class MockLocalizerExtensions
    {
        public static Mock<IStringLocalizer<T>> SetupLocalizedString<T>(
            this Mock<IStringLocalizer<T>> mockLocalizer, string key, string translatedString)
        {
            mockLocalizer
                .SetupGet(x => x[key])
                .Returns(new LocalizedString(string.Empty, translatedString));

            return mockLocalizer;
        }

        public static Mock<IStringLocalizer<T>> SetupLocalizedString<T>(
            this Mock<IStringLocalizer<T>> mockLocalizer,
            string key,
            object[] arguments,
            string translatedString)
        {
            mockLocalizer
                .SetupGet(x => x[key, arguments])
                .Returns(new LocalizedString(string.Empty, translatedString));

            return mockLocalizer;
        }
    }
}
