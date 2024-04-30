using Buttercup.EntityModel;

namespace Buttercup.Web.Models.Recipes;

public sealed record ShowRecipeViewModel(Recipe Recipe, Comment[] Comments);
