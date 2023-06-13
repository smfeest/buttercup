using Microsoft.Extensions.Localization;
using Moq;

namespace Buttercup.Web.TestUtils;

/// <summary>
/// Provides extension methods for string localizer mocks.
/// </summary>
internal static class MockLocalizerExtensions
{
    /// <summary>
    /// Specifies the string that should be returned when a named resource is requested without
    /// arguments.
    /// </summary>
    /// <typeparam name="T">
    /// The type that the localizer provides strings for.
    /// </typeparam>
    /// <param name="mockLocalizer">
    /// The mock localizer.
    /// </param>
    /// <param name="name">
    /// The resource name.
    /// </param>
    /// <param name="translatedString">
    /// The translated resource string.
    /// </param>
    /// <returns>
    /// The mock localizer, for chaining.
    /// </returns>
    public static Mock<IStringLocalizer<T>> SetupLocalizedString<T>(
        this Mock<IStringLocalizer<T>> mockLocalizer, string name, string translatedString)
    {
        mockLocalizer
            .SetupGet(x => x[name])
            .Returns(new LocalizedString(string.Empty, translatedString));

        return mockLocalizer;
    }

    /// <summary>
    /// Specifies the string that should be returned when a named resource is requested with
    /// specific arguments.
    /// </summary>
    /// <typeparam name="T">
    /// The type that the localizer provides strings for.
    /// </typeparam>
    /// <param name="mockLocalizer">
    /// The mock localizer.
    /// </param>
    /// <param name="name">
    /// The resource name.
    /// </param>
    /// <param name="arguments">
    /// The string arguments.
    /// </param>
    /// <param name="translatedString">
    /// The translated string.
    /// </param>
    /// <returns>
    /// The mock localizer, for chaining.
    /// </returns>
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
