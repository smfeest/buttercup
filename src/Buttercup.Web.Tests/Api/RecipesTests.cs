using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class RecipesTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    private const string RecipesQuery = """
        query {
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
        }
        """;

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
            CreatedByUser = IdName.From(recipe.CreatedByUser),
            recipe.Modified,
            ModifiedByUser = IdName.From(recipe.ModifiedByUser),
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
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }

    [Fact]
    public async Task FilteringRecipes()
    {
        var currentUser = this.ModelFactory.BuildUser() with { Name = "Auguste Gusteau" };
        var recipes = new[]
        {
            this.ModelFactory.BuildRecipe() with { Id = 1, Servings = 8 },
            this.ModelFactory.BuildRecipe() with { Id = 2, Servings = 4 },
            this.ModelFactory.BuildRecipe() with { Id = 3, Servings = 6 },
            this.ModelFactory.BuildRecipe() with {
                Id = 4,
                Servings = 2,
                CreatedByUser = currentUser
            },
        };
        await this.DatabaseFixture.InsertEntities(currentUser, recipes);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery("""
            query {
                recipes(
                    order: { id: ASC },
                    where: {
                        or: [
                            { servings: { gt: 7 } },
                            { createdByUser: { name: { eq: "Auguste Gusteau" } } }
                        ]
                    }
                ) { id }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);
        var filteredIds = dataElement.GetProperty("recipes").EnumerateArray().Select(
            u => u.GetProperty("id").GetInt64());

        Assert.Equal([1, 4], filteredIds);
    }

    [Fact]
    public async Task FilteringRecipesByAdminOnlyUserFieldWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery("""
            query {
                recipes(where: { createdByUser: { email: { eq: "foo@example.com" } } }) { id }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }

    [Fact]
    public async Task SortingRecipes()
    {
        var currentUser = this.ModelFactory.BuildUser();
        var recipes = new[]
        {
            this.ModelFactory.BuildRecipe() with { Id = 1, Title = "Recipe A" },
            this.ModelFactory.BuildRecipe() with { Id = 2, Title = "Recipe C" },
            this.ModelFactory.BuildRecipe() with { Id = 3, Title = "Recipe B" },
            this.ModelFactory.BuildRecipe() with { Id = 4, Title = "Recipe A" },
        };
        await this.DatabaseFixture.InsertEntities(currentUser, recipes);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery("""
            query {
                recipes(order: [{ title: DESC, id: ASC }]) { id title }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        Assert.Collection(
            dataElement.GetProperty("recipes").EnumerateArray(),
            r => JsonAssert.Equivalent(new { Id = 2, Title = "Recipe C" }, r),
            r => JsonAssert.Equivalent(new { Id = 3, Title = "Recipe B" }, r),
            r => JsonAssert.Equivalent(new { Id = 1, Title = "Recipe A" }, r),
            r => JsonAssert.Equivalent(new { Id = 4, Title = "Recipe A" }, r));
    }

    [Fact]
    public async Task SortingRecipesByAdminOnlyUserFieldsWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery("""
            query {
                recipes(order: { createdByUser: { email: ASC } }) { id }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }
}
