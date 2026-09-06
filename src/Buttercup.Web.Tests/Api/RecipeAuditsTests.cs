using Buttercup.EntityModel;
using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class RecipeAuditsTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    private const string RecipeAuditsQuery = """
        query {
            recipeAudits {
                nodes {
                    id
                    recipe { id title }
                    time
                    action
                    revision { id title }
                    actor { id name }
                    ipAddress
                }
            }
        }
        """;

    [Fact]
    public async Task QueryingRecipeAudits()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        var audits = new[]
        {
            this.ModelFactory.BuildRecipeAudit(
                this.ModelFactory.BuildRecipe(),
                RecipeAction.Create,
                setOptionalAttributes: true),
            this.ModelFactory.BuildRecipeAudit(
                this.ModelFactory.BuildRecipe(softDeleted: true),
                RecipeAction.Delete),
        };
        await this.DatabaseFixture.InsertEntities(currentUser, audits);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery(RecipeAuditsQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var expected = audits.Select(audit => new
        {
            audit.Id,
            Recipe = new { audit.Recipe!.Id, audit.Recipe.Title },
            audit.Time,
            Action = audit.Action.ToString().ToUpperInvariant(),
            Revision = audit.Revision is null ?
                null :
                new { audit.Revision.Id, audit.Revision.Title },
            Actor = IdName.From(audit.Actor),
            IpAddress = audit.IpAddress?.ToString(),
        });

        JsonAssert.Equivalent(
            expected, dataElement.GetProperty("recipeAudits").GetProperty("nodes"));
    }

    [Fact]
    public async Task QueryingRecipeAuditsWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery(RecipeAuditsQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(
            document.RootElement.GetProperty("data").GetProperty("recipeAudits"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }

    [Fact]
    public async Task FilteringRecipeAudits()
    {
        var currentUser = this.ModelFactory.BuildUser(true) with { IsAdmin = true };

        var recipe = this.ModelFactory.BuildRecipe();
        var audit = this.ModelFactory.BuildRecipeAudit(recipe, RecipeAction.Create);
        var auditOtherAction = this.ModelFactory.BuildRecipeAudit(recipe, RecipeAction.Update);
        var auditOtherRecipe = this.ModelFactory.BuildRecipeAudit(
            this.ModelFactory.BuildRecipe(), RecipeAction.Create);

        await this.DatabaseFixture.InsertEntities(
            currentUser, audit, auditOtherAction, auditOtherRecipe);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery("""
            query($recipeId: Long!) {
                recipeAudits(
                    where: {
                        and: [
                            { recipe: { id: { eq: $recipeId } } }
                            { action: { eq: CREATE } }
                        ]
                    }
                ) { nodes { id } }
            }
            """,
            new { recipeId = recipe.Id });
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var returnedIds = dataElement
            .GetProperty("recipeAudits")
            .GetProperty("nodes")
            .EnumerateArray()
            .Select(e => e.GetProperty("id").GetInt64());

        Assert.Equal(audit.Id, Assert.Single(returnedIds));
    }

    [Fact]
    public async Task SortingRecipeAudits()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };

        var recipe = this.ModelFactory.BuildRecipe();

        RecipeAudit AuditWithServings(int servings) => this.ModelFactory.BuildRecipeAudit(
            recipe, RecipeAction.Create) with
        {
            Revision = this.ModelFactory.BuildRecipeRevision(recipe) with { Servings = servings },
        };

        var audit4Servings = AuditWithServings(4);
        var audit2Servings = AuditWithServings(2);
        var audit6Servings = AuditWithServings(6);

        await this.DatabaseFixture.InsertEntities(currentUser, audit4Servings, audit2Servings, audit6Servings);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);

        using var response = await client.PostQuery("""
            query {
                recipeAudits(order: { revision: { servings: DESC } }) {
                    nodes { id }
                }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var returnedIds = dataElement
            .GetProperty("recipeAudits")
            .GetProperty("nodes")
            .EnumerateArray()
            .Select(e => e.GetProperty("id").GetInt64());

        Assert.Equal([audit6Servings.Id, audit4Servings.Id, audit2Servings.Id], returnedIds);
    }
}
