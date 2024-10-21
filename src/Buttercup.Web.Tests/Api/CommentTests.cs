using Buttercup.EntityModel;
using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class CommentTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task QueryingComment(bool setOptionalAttributes)
    {
        var currentUser = this.ModelFactory.BuildUser();
        var comment = this.ModelFactory.BuildComment(setOptionalAttributes, setRecipe: true);
        comment.Revisions.Add(
            CommentRevision.From(this.ModelFactory.BuildComment(setOptionalAttributes: true)));
        comment.Revisions.Add(
            CommentRevision.From(this.ModelFactory.BuildComment(setOptionalAttributes: false)));
        await this.DatabaseFixture.InsertEntities(currentUser, comment);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostCommentQuery(client, comment.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var expected = new
        {
            comment.Id,
            Recipe = new
            {
                comment.Recipe!.Id,
                comment.Recipe.Title,
            },
            Author = IdName.From(comment.Author),
            comment.Body,
            comment.Created,
            comment.Modified,
            comment.Deleted,
            DeletedByUser = IdName.From(comment.DeletedByUser),
            comment.Revision,
            Revisions = comment.Revisions.Select(revision => new
            {
                revision.Revision,
                revision.Created,
                revision.Body
            }),
        };

        JsonAssert.Equivalent(expected, dataElement.GetProperty("comment"));
    }

    [Fact]
    public async Task QueryingNonExistentComment()
    {
        var currentUser = this.ModelFactory.BuildUser();
        var comment = this.ModelFactory.BuildComment(setRecipe: true);
        await this.DatabaseFixture.InsertEntities(currentUser, comment);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostCommentQuery(client, this.ModelFactory.NextInt());
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        JsonAssert.ValueIsNull(dataElement.GetProperty("comment"));
    }

    [Fact]
    public async Task QueryingCommentWhenUnauthenticated()
    {
        var comment = this.ModelFactory.BuildComment(setRecipe: true);
        await this.DatabaseFixture.InsertEntities(comment);

        using var client = this.AppFactory.CreateClient();
        using var response = await PostCommentQuery(client, comment.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data").GetProperty("comment"));

        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }

    [Fact]
    public async Task QueryingDeletedCommentWhenAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        var comment = this.ModelFactory.BuildComment(setRecipe: true, softDeleted: true);
        await this.DatabaseFixture.InsertEntities(currentUser, comment);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostCommentQuery(client, comment.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var expected = new
        {
            comment.Id,
            Recipe = new
            {
                comment.Recipe!.Id,
                comment.Recipe.Title,
            },
            Author = IdName.From(comment.Author),
            comment.Body,
            comment.Created,
            comment.Modified,
            comment.Deleted,
            DeletedByUser = IdName.From(comment.DeletedByUser),
            comment.Revision,
            Revisions = Array.Empty<object>()
        };

        JsonAssert.Equivalent(expected, dataElement.GetProperty("comment"));
    }

    [Fact]
    public async Task QueryingDeletedCommentWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        var comment = this.ModelFactory.BuildComment(setRecipe: true, softDeleted: true);
        await this.DatabaseFixture.InsertEntities(currentUser, comment);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostCommentQuery(client, comment.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data").GetProperty("comment"));

        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }

    private static Task<HttpResponseMessage> PostCommentQuery(HttpClient client, long id) =>
        client.PostQuery("""
            query($id: Long!) {
                comment(id: $id) {
                    id
                    recipe { id title }
                    author { id name }
                    body
                    created
                    modified
                    deleted
                    deletedByUser { id name }
                    revision
                    revisions { revision created body }
                }
            }
            """,
            new { id });
}
