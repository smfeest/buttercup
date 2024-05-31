using Buttercup.Web.TestUtils;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class SoftDeleteRecipeTests(AppFactory<SoftDeleteRecipeTests> appFactory)
    : EndToEndTests<SoftDeleteRecipeTests>(appFactory)
{
    [Fact]
    public async Task DeletingRecipe()
    {
        var currentUser = this.ModelFactory.BuildUser();
        var recipe = this.ModelFactory.BuildRecipe();
        await this.DatabaseFixture.InsertEntities(currentUser, recipe);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostDeleteRecipeMutation(client, recipe.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var expected = new
        {
            Deleted = true,
            Recipe = new
            {
                recipe.Id,
                recipe.Title,
                DeletedByUser = new { currentUser.Id, currentUser.Name },
            },
        };
        var actual = ApiAssert.SuccessResponse(document).GetProperty("deleteRecipe");

        JsonAssert.Equivalent(expected, actual);
    }

    [Fact]
    public async Task DeletingRecipeWhenUnauthenticated()
    {
        using var client = this.AppFactory.CreateClient();
        using var response = await PostDeleteRecipeMutation(client, this.ModelFactory.NextInt());
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError("AUTH_NOT_AUTHORIZED", document);
    }

    [Fact]
    public async Task DeletingAlreadySoftDeletedRecipe()
    {
        var currentUser = this.ModelFactory.BuildUser();
        var recipe = this.ModelFactory.BuildRecipe(setOptionalAttributes: true, softDeleted: true);
        await this.DatabaseFixture.InsertEntities(currentUser, recipe);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostDeleteRecipeMutation(client, recipe.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var expected = new
        {
            Deleted = false,
            Recipe = new
            {
                recipe.Id,
                recipe.Title,
                DeletedByUser = new { recipe.DeletedByUser?.Id, recipe.DeletedByUser?.Name },
            },
        };
        var actual = ApiAssert.SuccessResponse(document).GetProperty("deleteRecipe");
        JsonAssert.Equivalent(expected, actual);
    }

    [Fact]
    public async Task DeletingNonExistentRecipe()
    {
        var currentUser = this.ModelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostDeleteRecipeMutation(client, this.ModelFactory.NextInt());
        using var document = await response.Content.ReadAsJsonDocument();

        var deleteRecipeElement = ApiAssert.SuccessResponse(document).GetProperty("deleteRecipe");

        Assert.False(deleteRecipeElement.GetProperty("deleted").GetBoolean());
        JsonAssert.ValueIsNull(deleteRecipeElement.GetProperty("recipe"));
    }

    private static Task<HttpResponseMessage> PostDeleteRecipeMutation(HttpClient client, long id) =>
        client.PostQuery(
            @"mutation($id: Long!) {
                deleteRecipe(input: { id: $id }) {
                    deleted
                    recipe {
                        id
                        title
                        deletedByUser { id name }
                    }
                }
            }",
            new { id });
}
