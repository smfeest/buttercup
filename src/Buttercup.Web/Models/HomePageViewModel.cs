using Buttercup.Models;

namespace Buttercup.Web.Models;

public class HomePageViewModel
{
    public IList<Recipe>? RecentlyAddedRecipes { get; init; }

    public IList<Recipe>? RecentlyUpdatedRecipes { get; init; }
}
