using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class HardDeleteRecipeTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    [Fact]
    public async Task DeletingRecipe()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        var recipe = this.ModelFactory.BuildRecipe();
        await this.DatabaseFixture.InsertEntities(currentUser, recipe);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostDeleteRecipeMutation(client, recipe.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var deleted = ApiAssert
            .SuccessResponse(document)
            .GetProperty("hardDeleteRecipe")
            .GetProperty("deleted")
            .GetBoolean();
        Assert.True(deleted);
    }

    [Fact]
    public async Task DeletingRecipeWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        var recipe = this.ModelFactory.BuildRecipe();
        await this.DatabaseFixture.InsertEntities(currentUser, recipe);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostDeleteRecipeMutation(client, recipe.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }

    [Fact]
    public async Task DeletingNonExistentRecipe()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostDeleteRecipeMutation(client, this.ModelFactory.NextInt());
        using var document = await response.Content.ReadAsJsonDocument();

        var deleted = ApiAssert
            .SuccessResponse(document)
            .GetProperty("hardDeleteRecipe")
            .GetProperty("deleted")
            .GetBoolean();
        Assert.False(deleted);
    }

    private static Task<HttpResponseMessage> PostDeleteRecipeMutation(HttpClient client, long id) =>
        client.PostQuery(
            @"mutation($id: Long!) {
                hardDeleteRecipe(input: { id: $id }) {
                    deleted
                }
            }",
            new { id });
}
