using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class DeletedCommentsTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    [Fact]
    public async Task SortingFilteringAndPagingDeletedComments()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        var deletedComments = new string[] { "Foo", "Bar", "Qux", "Baz", "Bar" }
            .Select(recipeTitle =>
                this.ModelFactory.BuildComment(setOptionalAttributes: true, softDeleted: true) with
                {
                    Recipe = this.ModelFactory.BuildRecipe() with { Title = recipeTitle }
                })
            .ToArray();
        var otherComment = this.ModelFactory.BuildComment(setRecipe: true);
        await this.DatabaseFixture.InsertEntities(currentUser, deletedComments, otherComment);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery("""
            query {
                deletedComments(
                    first: 3,
                    where: { recipe: { title: { neq: "Baz" } } }
                    order: { recipe: { title: ASC } }) {
                    nodes {
                        id
                        recipe { id title }
                        author { id name }
                        body
                        created
                        modified
                        revision
                    }
                }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var expected = new int[] { 1, 4, 0 }.Select(i =>
        {
            var deletedComment = deletedComments[i];

            return new
            {
                deletedComment.Id,
                Recipe = new { deletedComment.Recipe!.Id, deletedComment.Recipe.Title },
                Author = IdName.From(deletedComment.Author),
                deletedComment.Body,
                deletedComment.Created,
                deletedComment.Modified,
                deletedComment.Revision,
            };
        });

        JsonAssert.Equivalent(
            expected, dataElement.GetProperty("deletedComments").GetProperty("nodes"));
    }

    [Fact]
    public async Task QueryingDeletedCommentsWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery("""
            query {
                deletedComments {
                    nodes { id }
                }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(
            document.RootElement.GetProperty("data").GetProperty("deletedComments"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }

    // TODO: Test sort, filter and paging
}
