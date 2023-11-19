using Buttercup.Web.TestUtils;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class RecipesTests(AppFactory<RecipesTests> appFactory)
    : EndToEndTests<RecipesTests>(appFactory)
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
                createdByUser { id email }
                modified
                modifiedByUser { id email }
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

        await this.DatabaseFixture.InsertEntities(currentUser, recipes);

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
                new { recipe.CreatedByUser.Id, recipe.CreatedByUser.Email },
            recipe.Modified,
            ModifiedByUser = recipe.ModifiedByUser == null ?
                null :
                new { recipe.ModifiedByUser.Id, recipe.ModifiedByUser.Email },
            recipe.Revision
        });

        JsonAssert.ValueEquals(expected, dataElement.GetProperty("recipes"));
    }

    [Fact]
    public async Task QueryingRecipesWhenUnauthenticated()
    {
        using var client = this.AppFactory.CreateClient();
        using var response = await client.PostQuery(RecipesQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        Assert.False(document.RootElement.TryGetProperty("data", out _));
        ApiAssert.HasSingleError("AUTH_NOT_AUTHORIZED", document);
    }
}
