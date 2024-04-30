using Buttercup.EntityModel;

namespace Buttercup.Web.Models.Home;

public sealed record HomePageViewModel(
    IList<Recipe> RecentlyAddedRecipes, IList<Recipe> RecentlyUpdatedRecipes);
