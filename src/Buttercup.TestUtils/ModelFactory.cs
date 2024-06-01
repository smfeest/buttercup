using System.Globalization;
using Buttercup.EntityModel;

namespace Buttercup.TestUtils;

/// <summary>
/// Provides methods for instantiating dummy models in tests.
/// </summary>
public sealed class ModelFactory
{
    private int nextInt = Random.Shared.Next(0, 2);

    /// <summary>
    /// Instantiates a new <see cref="PasswordResetToken" /> object with unique property values.
    /// </summary>
    /// <returns>The new <see cref="PasswordResetToken" /> object.</returns>
    public PasswordResetToken BuildPasswordResetToken() =>
        new()
        {
            Token = this.NextString("token"),
            UserId = this.NextInt(),
            Created = this.NextDateTime(),
        };

    /// <summary>
    /// Instantiates a new <see cref="Recipe" /> object with unique property values.
    /// </summary>
    /// <param name="setOptionalAttributes">
    /// <c>true</c> if optional properties should be populated; <c>false</c> if they should be left
    /// null.
    /// </param>
    /// <param name="softDeleted">
    /// <c>true</c> if the recipe should be marked as soft-deleted; <c>false</c> otherwise.
    /// </param>
    /// <returns>The new <see cref="Recipe" /> object.</returns>
    public Recipe BuildRecipe(bool setOptionalAttributes = false, bool softDeleted = false)
    {
        var createdByUser = setOptionalAttributes ? this.BuildUser() : null;
        var modifiedByUser = setOptionalAttributes ? this.BuildUser() : null;
        var deletedByUser = softDeleted && setOptionalAttributes ? this.BuildUser() : null;

        return new()
        {
            Id = this.NextInt(),
            Title = this.NextString("title"),
            PreparationMinutes = setOptionalAttributes ? this.NextInt() : null,
            CookingMinutes = setOptionalAttributes ? this.NextInt() : null,
            Servings = setOptionalAttributes ? this.NextInt() : null,
            Ingredients = this.NextString("ingredients"),
            Method = this.NextString("method"),
            Suggestions = setOptionalAttributes ? this.NextString("suggestions") : null,
            Remarks = setOptionalAttributes ? this.NextString("remarks") : null,
            Source = setOptionalAttributes ? this.NextString("source") : null,
            Created = this.NextDateTime(),
            CreatedByUser = createdByUser,
            CreatedByUserId = createdByUser?.Id,
            Modified = this.NextDateTime(),
            ModifiedByUser = modifiedByUser,
            ModifiedByUserId = modifiedByUser?.Id,
            Deleted = softDeleted ? this.NextDateTime() : null,
            DeletedByUser = deletedByUser,
            DeletedByUserId = deletedByUser?.Id,
            Revision = this.NextInt(),
        };
    }

    /// <summary>
    /// Instantiates a new <see cref="User" /> object with unique property values.
    /// </summary>
    /// <param name="setOptionalAttributes">
    /// <c>true</c> if optional properties should be populated; <c>false</c> if they should be left
    /// null.
    /// </param>
    /// <returns>The new <see cref="User" /> object.</returns>
    public User BuildUser(bool setOptionalAttributes = false) => new()
    {
        Id = this.NextInt(),
        Name = this.NextString("name"),
        Email = this.NextEmail(),
        HashedPassword = setOptionalAttributes ? this.NextString("password-hash") : null,
        PasswordCreated = setOptionalAttributes ? this.NextDateTime() : null,
        SecurityStamp = this.NextInt().ToString("X8", CultureInfo.InvariantCulture),
        TimeZone = this.NextString("time-zone"),
        IsAdmin = this.NextBoolean(),
        Created = this.NextDateTime(),
        Modified = this.NextDateTime(),
        Revision = this.NextInt(),
    };

    /// <summary>
    /// Generates a boolean value.
    /// </summary>
    /// <returns>The boolean value.</returns>
    public bool NextBoolean() => this.NextInt() % 2 == 0;

    /// <summary>
    /// Generates a unique UTC date and time.
    /// </summary>
    /// <returns>The generated date and time.</returns>
    public DateTime NextDateTime() =>
        new DateTime(2000, 1, 2, 3, 4, 5, DateTimeKind.Utc)
            + (new TimeSpan(1, 2, 3, 4) * this.NextInt());

    /// <summary>
    /// Generates a unique email address.
    /// </summary>
    /// <returns>The generated email address.</returns>
    public string NextEmail() => $"{this.NextString("email")}@example.com";

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
