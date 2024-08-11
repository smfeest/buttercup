using Buttercup.Web.TestUtils;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class DeletedRecipesTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    private const string DeletedRecipesQuery =
        @"query {
            deletedRecipes {
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
                deleted
                deletedByUser { id name }
                revision
            }
        }";

    [Fact]
    public async Task QueryingDeletedRecipes()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        var deletedRecipe = this.ModelFactory.BuildRecipe(
            setOptionalAttributes: true, softDeleted: true);
        var otherRecipe = this.ModelFactory.BuildRecipe();
        await this.DatabaseFixture.InsertEntities(currentUser, deletedRecipe, otherRecipe);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery(DeletedRecipesQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var expected = new[]
        {
            new
            {
                deletedRecipe.Id,
                deletedRecipe.Title,
                deletedRecipe.PreparationMinutes,
                deletedRecipe.CookingMinutes,
                deletedRecipe.Servings,
                deletedRecipe.Ingredients,
                deletedRecipe.Method,
                deletedRecipe.Suggestions,
                deletedRecipe.Remarks,
                deletedRecipe.Source,
                deletedRecipe.Created,
                CreatedByUser = IdName.From(deletedRecipe.CreatedByUser),
                deletedRecipe.Modified,
                ModifiedByUser = IdName.From(deletedRecipe.ModifiedByUser),
                deletedRecipe.Deleted,
                DeletedByUser = IdName.From(deletedRecipe.DeletedByUser),
                deletedRecipe.Revision
            }
        };

        JsonAssert.Equivalent(expected, dataElement.GetProperty("deletedRecipes"));
    }

    [Fact]
    public async Task QueryingDeletedRecipesWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery(DeletedRecipesQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError("AUTH_NOT_AUTHORIZED", document);
    }

    [Fact]
    public async Task SortingDeletedRecipes()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        var recipes = new[]
        {
            this.ModelFactory.BuildRecipe(softDeleted: true) with { Id = 1, Title = "Recipe A" },
            this.ModelFactory.BuildRecipe(softDeleted: true) with { Id = 2, Title = "Recipe C" },
            this.ModelFactory.BuildRecipe(softDeleted: true) with { Id = 3, Title = "Recipe B" },
        };
        await this.DatabaseFixture.InsertEntities(currentUser, recipes);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery(@"query {
            deletedRecipes(order: [{ title: ASC }]) { id }
        }");
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);
        var sortedIds = dataElement.GetProperty("deletedRecipes").EnumerateArray().Select(
            u => u.GetProperty("id").GetInt64());

        Assert.Equal([1, 3, 2], sortedIds);
    }
}
