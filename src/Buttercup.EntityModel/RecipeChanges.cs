using System.Text.Json.Serialization;

namespace Buttercup.EntityModel;

/// <summary>
/// Represents a set of recipe attribute changes.
/// </summary>
public sealed record RecipeChanges
{
    /// <summary>
    /// Gets or sets the recipe title change, if any.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(ChangedValueJsonConverter<string>))]
    public ChangedValue<string>? Title { get; set; }

    /// <summary>
    /// Gets or sets the preparation time change, if any.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(ChangedValueJsonConverter<int?>))]
    public ChangedValue<int?>? PreparationMinutes { get; set; }

    // /// <summary>
    // /// Gets or sets the cooking time in minutes.
    // /// </summary>
    // public int? CookingMinutes { get; set; }

    // /// <summary>
    // /// Gets or sets the number of servings.
    // /// </summary>
    // public int? Servings { get; set; }

    // /// <summary>
    // /// Gets or sets the ingredients.
    // /// </summary>
    // [Column(TypeName = "text")]
    // public required string Ingredients { get; set; }

    // /// <summary>
    // /// Gets or sets the method.
    // /// </summary>
    // [Column(TypeName = "text")]
    // public required string Method { get; set; }

    // /// <summary>
    // /// Gets or sets the suggestions for the recipe.
    // /// </summary>
    // [Column(TypeName = "text")]
    // public string? Suggestions { get; set; }

    // /// <summary>
    // /// Gets or sets the remarks for the recipe.
    // /// </summary>
    // [Column(TypeName = "text")]
    // public string? Remarks { get; set; }

    // /// <summary>
    // /// Gets or sets the source of the recipe.
    // /// </summary>
    // [StringLength(250)]
    // public string? Source { get; set; }
}
