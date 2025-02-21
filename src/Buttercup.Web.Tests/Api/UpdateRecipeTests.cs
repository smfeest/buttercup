using Buttercup.Application;
using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class UpdateRecipeTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdatingRecipe(bool setOptionalAttributes)
    {
        var currentUser = this.ModelFactory.BuildUser();
        var recipe = this.ModelFactory.BuildRecipe(true);
        await this.DatabaseFixture.InsertEntities(currentUser, recipe);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);

        var attributes = new RecipeAttributes(this.ModelFactory.BuildRecipe(setOptionalAttributes));

        using var response = await PostUpdateRecipeMutation(
            client, recipe.Id, recipe.Revision, attributes);
        using var document = await response.Content.ReadAsJsonDocument();

        var updateRecipeElement = ApiAssert.SuccessResponse(document).GetProperty("updateRecipe");
        var recipeElement = updateRecipeElement.GetProperty("recipe");

        var expected = new
        {
            recipe.Id,
            attributes.Title,
            attributes.PreparationMinutes,
            attributes.CookingMinutes,
            attributes.Servings,
            attributes.Ingredients,
            attributes.Method,
            attributes.Suggestions,
            attributes.Remarks,
            attributes.Source,
            recipe.Created,
            CreatedByUser = IdName.From(recipe.CreatedByUser),
            ModifiedByUser = IdName.From(currentUser),
            Revision = recipe.Revision + 1
        };
        JsonAssert.Equivalent(expected, recipeElement);

        JsonAssert.ValueIsNull(updateRecipeElement.GetProperty("errors"));
    }

    [Fact]
    public async Task UpdatingRecipeWhenUnauthenticated()
    {
        var recipe = this.ModelFactory.BuildRecipe();

        using var client = this.AppFactory.CreateClient();
        using var response = await PostUpdateRecipeMutation(
            client, recipe.Id, recipe.Revision, new(recipe));
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthenticated, document);
    }

    [Fact]
    public async Task UpdatingNonExistentRecipe()
    {
        var currentUser = this.ModelFactory.BuildUser();
        var recipe = this.ModelFactory.BuildRecipe();
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);

        using var response = await PostUpdateRecipeMutation(
            client, recipe.Id, recipe.Revision, new(recipe));
        using var document = await response.Content.ReadAsJsonDocument();

        var updateRecipeElement = ApiAssert.SuccessResponse(document).GetProperty("updateRecipe");

        JsonAssert.ValueIsNull(updateRecipeElement.GetProperty("recipe"));

        var expectedErrors = new[]
        {
            new
            {
                __typename = "NotFoundError",
                Message = $"Recipe/{recipe.Id} not found",
            },
        };
        JsonAssert.Equivalent(expectedErrors, updateRecipeElement.GetProperty("errors"));
    }

    [Fact]
    public async Task UpdatingSoftDeletedRecipe()
    {
        var currentUser = this.ModelFactory.BuildUser();
        var recipe = this.ModelFactory.BuildRecipe(softDeleted: true);
        await this.DatabaseFixture.InsertEntities(currentUser, recipe);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);

        using var response = await PostUpdateRecipeMutation(
            client, recipe.Id, recipe.Revision, new(recipe));
        using var document = await response.Content.ReadAsJsonDocument();

        var updateRecipeElement = ApiAssert.SuccessResponse(document).GetProperty("updateRecipe");

        JsonAssert.ValueIsNull(updateRecipeElement.GetProperty("recipe"));

        var expectedErrors = new[]
        {
            new
            {
                __typename = "SoftDeletedError",
                Message = $"Cannot update soft-deleted recipe {recipe.Id}",
            },
        };
        JsonAssert.Equivalent(expectedErrors, updateRecipeElement.GetProperty("errors"));
    }

    [Fact]
    public async Task UpdatingRecipeWithStaleBaseRevision()
    {
        var currentUser = this.ModelFactory.BuildUser();
        var recipe = this.ModelFactory.BuildRecipe();
        await this.DatabaseFixture.InsertEntities(currentUser, recipe);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);

        using var response = await PostUpdateRecipeMutation(
            client, recipe.Id, recipe.Revision - 1, new(this.ModelFactory.BuildRecipe()));
        using var document = await response.Content.ReadAsJsonDocument();

        var updateRecipeElement = ApiAssert.SuccessResponse(document).GetProperty("updateRecipe");

        JsonAssert.ValueIsNull(updateRecipeElement.GetProperty("recipe"));

        var expectedErrors = new[]
        {
            new
            {
                __typename = "ConcurrencyError",
                Message = $"Revision {recipe.Revision - 1} does not match current revision {recipe.Revision}",
            },
        };
        JsonAssert.Equivalent(expectedErrors, updateRecipeElement.GetProperty("errors"));
    }

    [Fact]
    public async Task UpdatingRecipeWithInvalidAttributes()
    {
        var currentUser = this.ModelFactory.BuildUser();
        var recipe = this.ModelFactory.BuildRecipe(true);
        await this.DatabaseFixture.InsertEntities(currentUser, recipe);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);

        var attributes = new RecipeAttributes(recipe) with
        {
            Ingredients = string.Empty,
            CookingMinutes = -1,
        };

        using var response = await PostUpdateRecipeMutation(
            client, recipe.Id, recipe.Revision, attributes);
        using var document = await response.Content.ReadAsJsonDocument();

        var updateRecipeElement = ApiAssert.SuccessResponse(document).GetProperty("updateRecipe");

        JsonAssert.ValueIsNull(updateRecipeElement.GetProperty("recipe"));

        var expectedErrors = new[]
        {
            new
            {
                __typename = "ValidationError",
                Message = "This field is limited to values between 0 and 2147483647",
                Path = new string[] { "input", "attributes", "cookingMinutes" },
                Code = "OUT_OF_RANGE",
            },
            new
            {
                __typename = "ValidationError",
                Message = "This field is required",
                Path = new string[] { "input", "attributes", "ingredients" },
                Code = "REQUIRED",
            },
        };
        JsonAssert.Equivalent(expectedErrors, updateRecipeElement.GetProperty("errors"));
    }

    private static Task<HttpResponseMessage> PostUpdateRecipeMutation(
        HttpClient client, long id, int baseRevision, RecipeAttributes attributes) =>
        client.PostQuery("""
            mutation($input: UpdateRecipeInput!) {
                updateRecipe(input: $input) {
                    recipe {
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
                        modifiedByUser { id name }
                        revision
                    }
                    errors {
                        __typename
                        ... on Error {
                            message
                        }
                        ... on ValidationError {
                            path
                            code
                        }
                    }
                }
            }
            """,
            new { input = new { id, baseRevision, attributes } });
}
