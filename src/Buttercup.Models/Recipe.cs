namespace Buttercup.Models;

/// <summary>
/// Represents a recipe.
/// </summary>
/// <param name="Id">
/// The recipe ID.
/// </param>
/// <param name="Title">
/// The title.
/// </param>
/// <param name="PreparationMinutes">
/// The preparation time in minutes.
/// </param>
/// <param name="CookingMinutes">
/// The cooking time in minutes.
/// </param>
/// <param name="Servings">
/// The number of servings.
/// </param>
/// <param name="Ingredients">
/// The ingredients.
/// </param>
/// <param name="Method">
/// The method.
/// </param>
/// <param name="Suggestions">
/// The suggestions.
/// </param>
/// <param name="Remarks">
/// The remarks.
/// </param>
/// <param name="Source">
/// The source.
/// </param>
/// <param name="Created">
/// The date and time at which the record was created.
/// </param>
/// <param name="CreatedByUserId">
/// The user ID of the user who created the record.
/// </param>
/// <param name="Modified">
/// The date and time at which the record was last modified.
/// </param>
/// <param name="ModifiedByUserId">
/// The user ID of the user who last modified the record.
/// </param>
/// <param name="Revision">
/// The revision number.
/// </param>
public sealed record Recipe(
    long Id,
    string? Title,
    int? PreparationMinutes,
    int? CookingMinutes,
    int? Servings,
    string? Ingredients,
    string? Method,
    string? Suggestions,
    string? Remarks,
    string? Source,
    DateTime Created,
    long? CreatedByUserId,
    DateTime Modified,
    long? ModifiedByUserId,
    int Revision);
