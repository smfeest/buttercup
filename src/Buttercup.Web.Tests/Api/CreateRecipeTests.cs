using Buttercup.Application;
using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class CreateRecipeTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreatingRecipe(bool setOptionalAttributes)
    {
        var currentUser = this.ModelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);

        var attributes = new RecipeAttributes(this.ModelFactory.BuildRecipe(setOptionalAttributes));

        using var response = await PostCreateRecipeMutation(client, attributes);
        using var document = await response.Content.ReadAsJsonDocument();

        var createRecipeElement = ApiAssert.SuccessResponse(document).GetProperty("createRecipe");
        var recipeElement = createRecipeElement.GetProperty("recipe");
        var id = recipeElement.GetProperty("id").GetInt64();

        var expected = new
        {
            id,
            attributes.Title,
            attributes.PreparationMinutes,
            attributes.CookingMinutes,
            attributes.Servings,
            attributes.Ingredients,
            attributes.Method,
            attributes.Suggestions,
            attributes.Remarks,
            attributes.Source,
            CreatedByUser = IdName.From(currentUser),
            ModifiedByUser = IdName.From(currentUser),
            Revision = 0
        };
        JsonAssert.Equivalent(expected, recipeElement);

        JsonAssert.ValueIsNull(createRecipeElement.GetProperty("errors"));
    }

    [Fact]
    public async Task CreatingRecipeWhenUnauthenticated()
    {
        var attributes = new RecipeAttributes(this.ModelFactory.BuildRecipe());

        using var client = this.AppFactory.CreateClient();
        using var response = await PostCreateRecipeMutation(client, attributes);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }

    [Fact]
    public async Task CreatingRecipeWithInvalidAttributes()
    {
        var currentUser = this.ModelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);

        var attributes = new RecipeAttributes(this.ModelFactory.BuildRecipe()) with
        {
            Method = string.Empty,
            Servings = -5,
        };

        using var response = await PostCreateRecipeMutation(client, attributes);
        using var document = await response.Content.ReadAsJsonDocument();

        var createRecipeElement = ApiAssert.SuccessResponse(document).GetProperty("createRecipe");

        JsonAssert.ValueIsNull(createRecipeElement.GetProperty("recipe"));

        var expectedErrors = new[]
        {
            new
            {
                Message = "This field is limited to values between 1 and 2147483647",
                Path = new string[] { "input", "attributes", "servings" },
                Code = "OUT_OF_RANGE",
            },
            new
            {
                Message = "This field is required",
                Path = new string[] { "input", "attributes", "method" },
                Code = "REQUIRED",
            },
        };
        JsonAssert.Equivalent(expectedErrors, createRecipeElement.GetProperty("errors"));
    }

    private static Task<HttpResponseMessage> PostCreateRecipeMutation(
        HttpClient client, RecipeAttributes attributes) =>
        client.PostQuery(
            @"mutation($attributes: RecipeAttributesInput!) {
                createRecipe(input: { attributes: $attributes }) {
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
                        createdByUser { id name }
                        modifiedByUser { id name }
                        revision
                    }
                    errors {
                        ... on ValidationError {
                            message
                            path
                            code
                        }
                    }
                }
            }",
            new { attributes });
}
