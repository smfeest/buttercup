using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Buttercup.Models;

namespace Buttercup.Web.Models
{
    [SuppressMessage(
        "Usage",
        "CA2227:CollectionPropertiesShouldBeReadOnly",
        Justification = "Analyzer does not yet skip over init-only properties")]
    public class HomePageViewModel
    {
        public IList<Recipe>? RecentlyAddedRecipes { get; init; }

        public IList<Recipe>? RecentlyUpdatedRecipes { get; init; }
    }
}
