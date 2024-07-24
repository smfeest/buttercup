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
                CreatedByUser = new
                {
                    deletedRecipe.CreatedByUser!.Id,
                    deletedRecipe.CreatedByUser.Name,
                },
                deletedRecipe.Modified,
                ModifiedByUser = new
                {
                    deletedRecipe.ModifiedByUser!.Id,
                    deletedRecipe.ModifiedByUser.Name,
                },
                deletedRecipe.Deleted,
                DeletedByUser = new
                {
                    deletedRecipe.DeletedByUser!.Id,
                    deletedRecipe.DeletedByUser.Name,
                },
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
}
