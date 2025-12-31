using Buttercup.Application;
using Buttercup.EntityModel;
using Microsoft.AspNetCore.Http;

namespace Buttercup.Web.Models.Recipes;

public sealed record EditRecipeViewModel(
    long Id,
    RecipeAttributes Attributes,
    int BaseRevision,
    IFormFile? Photo = null,
    string? ExistingPhotoUrl = null)
{
    public static EditRecipeViewModel ForRecipe(Recipe recipe) => new(
        recipe.Id, new(recipe), recipe.Revision, ExistingPhotoUrl: recipe.PhotoUrl);
}
