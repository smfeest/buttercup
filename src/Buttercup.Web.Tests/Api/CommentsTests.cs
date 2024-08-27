using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class CommentsTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    private const string CommentsQuery = """
        query {
            comments {
                id
                recipe { id title }
                author { id name }
                body
                created
                modified
                revision
                revisions { revision created body }
            }
        }
        """;

    [Fact]
    public async Task QueryingComments()
    {
        var currentUser = this.ModelFactory.BuildUser();
        var comments = new[]
        {
            this.ModelFactory.BuildComment(setOptionalAttributes: true, setRecipe: true),
            this.ModelFactory.BuildComment(setOptionalAttributes: false, setRecipe: true),
        };
        var deletedComment = this.ModelFactory.BuildRecipe(softDeleted: true);
        await this.DatabaseFixture.InsertEntities(currentUser, comments, deletedComment);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery(CommentsQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var expected = comments.Select(comment => new
        {
            comment.Id,
            Recipe = new { comment.Recipe!.Id, comment.Recipe.Title },
            Author = IdName.From(comment.Author),
            comment.Body,
            comment.Created,
            comment.Modified,
            comment.Revision,
            Revisions = comment.Revisions.Select(revision => new
            {
                revision.Revision,
                revision.Created,
                revision.Body
            }),
        });

        JsonAssert.Equivalent(expected, dataElement.GetProperty("comments"));
    }

    [Fact]
    public async Task QueryingCommentsWhenUnauthenticated()
    {
        using var client = this.AppFactory.CreateClient();
        using var response = await client.PostQuery(CommentsQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }
}
