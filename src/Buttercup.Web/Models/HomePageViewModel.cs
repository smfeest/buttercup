using System.Collections.Generic;
using Buttercup.Models;

namespace Buttercup.Web.Models
{
    public class HomePageViewModel
    {
        public IList<Recipe>? RecentlyAddedRecipes { get; set; }

        public IList<Recipe>? RecentlyUpdatedRecipes { get; set; }
    }
}
