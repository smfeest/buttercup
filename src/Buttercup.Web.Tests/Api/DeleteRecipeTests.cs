using Buttercup.Web.TestUtils;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class DeleteRecipeTests(AppFactory<DeleteRecipeTests> appFactory)
    : EndToEndTests<DeleteRecipeTests>(appFactory)
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

        var deleted = ApiAssert
            .SuccessResponse(document)
            .GetProperty("deleteRecipe")
            .GetProperty("deleted")
            .GetBoolean();
        Assert.True(deleted);
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
    public async Task DeletingNonExistentRecipe()
    {
        var currentUser = this.ModelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostDeleteRecipeMutation(client, this.ModelFactory.NextInt());
        using var document = await response.Content.ReadAsJsonDocument();

        var deleted = ApiAssert
            .SuccessResponse(document)
            .GetProperty("deleteRecipe")
            .GetProperty("deleted")
            .GetBoolean();
        Assert.False(deleted);
    }

    private static Task<HttpResponseMessage> PostDeleteRecipeMutation(HttpClient client, long id) =>
        client.PostQuery(
            @"mutation($id: Long!) {
                deleteRecipe(input: { id: $id }) {
                    deleted
                }
            }",
            new { id });
}
