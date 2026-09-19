using Buttercup.EntityModel;

namespace Buttercup.Web.Models.Recipes;

public sealed record RecipeForIndex(Recipe Recipe, int CommentCount)
{
    public long Id => this.Recipe.Id;
    public string Title => this.Recipe.Title;
    public DateTime Created => this.Recipe.Created;
    public DateTime Modified => this.Recipe.Modified;
}
