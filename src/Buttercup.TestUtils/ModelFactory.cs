using System.Globalization;
using Buttercup.EntityModel;
using Buttercup.Models;

namespace Buttercup.TestUtils;

/// <summary>
/// Provides methods for instantiating dummy models in tests.
/// </summary>
public class ModelFactory
{
    private int nextInt = 1;

    /// <summary>
    /// Instantiates a new <see cref="Recipe" /> object with unique property values.
    /// </summary>
    /// <param name="setOptionalAttributes">
    /// <c>true</c> if optional properties should be populated; <c>false</c> if they should be left
    /// null.
    /// </param>
    /// <returns>The new <see cref="Recipe" /> object.</returns>
    public Recipe BuildRecipe(bool setOptionalAttributes = false) => new(
        this.NextInt(),
        this.NextString("title"),
        setOptionalAttributes ? this.NextInt() : null,
        setOptionalAttributes ? this.NextInt() : null,
        setOptionalAttributes ? this.NextInt() : null,
        this.NextString("ingredients"),
        this.NextString("method"),
        setOptionalAttributes ? this.NextString("suggestions") : null,
        setOptionalAttributes ? this.NextString("remarks") : null,
        setOptionalAttributes ? this.NextString("source") : null,
        this.NextDateTime(),
        setOptionalAttributes ? this.NextInt() : null,
        this.NextDateTime(),
        setOptionalAttributes ? this.NextInt() : null,
        this.NextInt());

    /// <summary>
    /// Instantiates a new <see cref="RecipeAttributes" /> object with unique property values.
    /// </summary>
    /// <param name="setOptionalAttributes">
    /// <c>true</c> if optional properties should be populated; <c>false</c> if they should be left
    /// null.
    /// </param>
    /// <returns>The new <see cref="RecipeAttributes" /> object.</returns>
    public RecipeAttributes BuildRecipeAttributes(bool setOptionalAttributes = false) =>
        new(BuildRecipe(setOptionalAttributes));

    /// <summary>
    /// Instantiates a new <see cref="User" /> object with unique property values.
    /// </summary>
    /// <param name="setOptionalAttributes">
    /// <c>true</c> if optional properties should be populated; <c>false</c> if they should be left
    /// null.
    /// </param>
    /// <returns>The new <see cref="User" /> object.</returns>
    public User BuildUser(bool setOptionalAttributes = false) => new(
        this.NextInt(),
        this.NextString("name"),
        $"user-{this.NextInt()}@example.com",
        setOptionalAttributes ? this.NextString("password-hash") : null,
        setOptionalAttributes ? this.NextDateTime() : null,
        this.NextInt().ToString("X8", CultureInfo.InvariantCulture),
        this.NextString("time-zone"),
        this.NextDateTime(),
        this.NextDateTime(),
        this.NextInt());

    /// <summary>
    /// Generates a unique UTC date and time.
    /// </summary>
    /// <returns>The generated date and time.</returns>
    public DateTime NextDateTime() =>
        new DateTime(2000, 1, 2, 3, 4, 5, DateTimeKind.Utc)
            + (new TimeSpan(1, 2, 3, 4) * this.NextInt());

    /// <summary>
    /// Generates a unique integer value.
    /// </summary>
    /// <returns>The generated integer value.</returns>
    public int NextInt() => this.nextInt++;

    /// <summary>
    /// Generates a unique string value.
    /// </summary>
    /// <param name="prefix">The prefix to be included in the string.</param>
    /// <returns>The generated string value.</returns>
    public string NextString(string prefix) => $"{prefix}-{this.NextInt()}";
}
