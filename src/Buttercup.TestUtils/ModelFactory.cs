using System.Globalization;
using System.Net;
using Buttercup.EntityModel;

namespace Buttercup.TestUtils;

/// <summary>
/// Provides methods for instantiating dummy models in tests.
/// </summary>
public sealed class ModelFactory
{
    private int nextInt = Random.Shared.Next(0, 2);

    /// <summary>
    /// Instantiates a new <see cref="Comment" /> object with unique property values.
    /// </summary>
    /// <param name="setOptionalAttributes">
    /// <b>true</b> if optional properties should be populated; <b>false</b> if they should be left
    /// null.
    /// </param>
    /// <param name="setRecipe">
    /// <b>true</b> if a <see cref="Comment.Recipe"/> should be populated and <see
    /// cref="Comment.RecipeId"/> set to match; <b>false</b> if <see cref="Comment.Recipe"/> and
    /// <see cref="Comment.RecipeId"/> should be left null and zero.
    /// </param>
    /// <param name="softDeleted">
    /// <b>true</b> if the commend should be marked as soft-deleted; <b>false</b> otherwise.
    /// </param>
    /// <returns>The new <see cref="Comment" /> object.</returns>
    public Comment BuildComment(
        bool setOptionalAttributes = false, bool setRecipe = false, bool softDeleted = false)
    {
        var recipe = setRecipe ? this.BuildRecipe() : null;
        var author = setOptionalAttributes ? this.BuildUser() : null;
        var deletedByUser = softDeleted && setOptionalAttributes ? this.BuildUser() : null;

        return new()
        {
            Id = this.NextInt(),
            Recipe = recipe,
            RecipeId = recipe?.Id ?? 0,
            Author = author,
            AuthorId = author?.Id,
            Body = this.NextString("comment-body"),
            Created = this.NextDateTime(),
            Modified = this.NextDateTime(),
            Deleted = softDeleted ? this.NextDateTime() : null,
            DeletedByUser = deletedByUser,
            DeletedByUserId = deletedByUser?.Id,
            Revision = this.NextInt(),
        };
    }

    /// <summary>
    /// Instantiates a new <see cref="PasswordResetToken" /> object with unique property values.
    /// </summary>
    /// <param name="user">
    /// The user for <see cref="PasswordResetToken.User"/> and <see
    /// cref="PasswordResetToken.UserId"/>.
    /// </param>
    /// <returns>The new <see cref="PasswordResetToken" /> object.</returns>
    public PasswordResetToken BuildPasswordResetToken(User user) =>
        new()
        {
            Token = this.NextString("token"),
            User = user,
            UserId = user.Id,
            Created = this.NextDateTime(),
        };

    /// <summary>
    /// Instantiates a new <see cref="Recipe" /> object with unique property values.
    /// </summary>
    /// <param name="setOptionalAttributes">
    /// <b>true</b> if optional properties should be populated; <b>false</b> if they should be left
    /// null.
    /// </param>
    /// <param name="softDeleted">
    /// <b>true</b> if the recipe should be marked as soft-deleted; <b>false</b> otherwise.
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
    /// <b>true</b> if optional properties should be populated; <b>false</b> if they should be left
    /// null.
    /// </param>
    /// <param name="deactivated">
    /// <b>true</b> if the user should be marked as deactivated; <b>false</b> otherwise.
    /// </param>
    /// <returns>The new <see cref="User" /> object.</returns>
    public User BuildUser(bool setOptionalAttributes = false, bool deactivated = false) => new()
    {
        Id = this.NextInt(),
        Name = this.NextString("name"),
        Email = this.NextEmail(),
        HashedPassword = setOptionalAttributes ? this.NextString("password-hash") : null,
        PasswordCreated = setOptionalAttributes ? this.NextDateTime() : null,
        SecurityStamp = this.NextToken(8),
        TimeZone = this.NextString("time-zone"),
        IsAdmin = this.NextBoolean(),
        Created = this.NextDateTime(),
        Modified = this.NextDateTime(),
        Deactivated = deactivated ? this.NextDateTime() : null,
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
    /// Generates a unique IP address.
    /// </summary>
    /// <returns>The generated IP address.</returns>
    public IPAddress NextIpAddress() => new(this.NextInt());

    /// <summary>
    /// Generates a unique string value.
    /// </summary>
    /// <param name="prefix">The prefix to be included in the string.</param>
    /// <returns>The generated string value.</returns>
    public string NextString(string prefix) => $"{prefix}-{this.NextInt()}";

    /// <summary>
    /// Generates a unique token string of the specified length.
    /// </summary>
    /// <param name="length">The token length.</param>
    /// <returns>The generated token.</returns>
    public string NextToken(int length) =>
        this.NextInt().ToString($"X{length}", CultureInfo.InvariantCulture);
}
