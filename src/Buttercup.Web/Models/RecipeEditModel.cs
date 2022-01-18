using Buttercup.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Buttercup.Web.Models;

public sealed record RecipeEditModel
{
    public RecipeEditModel()
    {
    }

    public RecipeEditModel(Recipe recipe)
    {
        this.Id = recipe.Id;
        this.Attributes = new(recipe);
        this.Revision = recipe.Revision;
    }

    [BindNever]
    public long Id { get; init; }

    public RecipeAttributes Attributes { get; init; } = new();

    public int Revision { get; init; }

    public Recipe ToRecipe() => new(
        this.Id,
        this.Attributes.Title,
        this.Attributes.PreparationMinutes,
        this.Attributes.CookingMinutes,
        this.Attributes.Servings,
        this.Attributes.Ingredients,
        this.Attributes.Method,
        this.Attributes.Suggestions,
        this.Attributes.Remarks,
        this.Attributes.Source,
        new(),
        null,
        new(),
        null,
        this.Revision);
}
