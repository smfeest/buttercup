using Buttercup.Web.TestUtils;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class RecipesTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    private const string RecipesQuery =
        @"query {
            recipes {
                id
                title
                preparationMinutes
                cookingMinutes
                servings
                ingredients
                method
                suggestions
                remarks
                source
                created
                createdByUser { id name }
                modified
                modifiedByUser { id name }
                revision
            }
        }";

    [Fact]
    public async Task QueryingRecipes()
    {
        var currentUser = this.ModelFactory.BuildUser();
        var recipes = new[]
        {
            this.ModelFactory.BuildRecipe(true),
            this.ModelFactory.BuildRecipe(false),
        };
        var deletedRecipe = this.ModelFactory.BuildRecipe(softDeleted: true);
        await this.DatabaseFixture.InsertEntities(currentUser, recipes, deletedRecipe);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery(RecipesQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var expected = recipes.Select(recipe => new
        {
            recipe.Id,
            recipe.Title,
            recipe.PreparationMinutes,
            recipe.CookingMinutes,
            recipe.Servings,
            recipe.Ingredients,
            recipe.Method,
            recipe.Suggestions,
            recipe.Remarks,
            recipe.Source,
            recipe.Created,
            CreatedByUser = recipe.CreatedByUser == null ?
                null :
                new { recipe.CreatedByUser.Id, recipe.CreatedByUser.Name },
            recipe.Modified,
            ModifiedByUser = recipe.ModifiedByUser == null ?
                null :
                new { recipe.ModifiedByUser.Id, recipe.ModifiedByUser.Name },
            recipe.Revision
        });

        JsonAssert.Equivalent(expected, dataElement.GetProperty("recipes"));
    }

    [Fact]
    public async Task QueryingRecipesWhenUnauthenticated()
    {
        using var client = this.AppFactory.CreateClient();
        using var response = await client.PostQuery(RecipesQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError("AUTH_NOT_AUTHORIZED", document);
    }
}
