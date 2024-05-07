using System.Globalization;
using Microsoft.Extensions.Localization;

namespace Buttercup.TestUtils;

/// <summary>
/// A fake string localizer that retrieves resources from a dictionary.
/// </summary>
/// <typeparam name="T">
/// The type to provide strings for.
/// </typeparam>
public sealed class DictionaryLocalizer<T>() : IStringLocalizer<T>
{
    /// <summary>
    /// Gets the backing dictionary.
    /// </summary>
    public Dictionary<string, string> Strings { get; } = [];

    /// <inheritdoc />
    public LocalizedString this[string name] =>
        this.Strings.TryGetValue(name, out var value) ? new(name, value) : new(name, name, true);

    /// <inheritdoc />
    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            this.Strings.TryGetValue(name, out var value);

            return new(
                name,
                string.Format(CultureInfo.CurrentCulture, value ?? name, arguments),
                value == null);
        }
    }

    /// <summary>
    /// Adds a string to the backing dictionary.
    /// </summary>
    /// <param name="name">
    /// The string name.
    /// </param>
    /// <param name="value">
    /// The string value.
    /// </param>
    /// <returns>
    /// This localizer, for chaining.
    /// </returns>
    public DictionaryLocalizer<T> Add(string name, string value)
    {
        this.Strings.Add(name, value);
        return this;
    }

    /// <inheritdoc />
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
        this.Strings.Select(kvp => new LocalizedString(kvp.Key, kvp.Value));
}
