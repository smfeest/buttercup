using Buttercup.Models;

namespace Buttercup.Web.Models;

public sealed record HomePageViewModel(
    IList<Recipe> RecentlyAddedRecipes, IList<Recipe> RecentlyUpdatedRecipes);
