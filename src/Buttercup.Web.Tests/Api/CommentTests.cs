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
        var audit = this.ModelFactory.BuildCommentAudit(comment, CommentAction.Create, setOptionalAttributes);

        await this.DatabaseFixture.InsertEntities(currentUser, comment, audit);

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
            Audits = new[]
            {
                new
                {
                    audit.Id,
                    audit.Time,
                    Action = "CREATE",
                    Revision = audit.Revision is null ? null : new { audit.Revision.Id, audit.Revision.Body },
                    Actor = IdName.From(audit.Actor),
                },
            },
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

        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthenticated, document);
    }

    [Fact]
    public async Task QueryingDeletedCommentWhenAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        var comment = this.ModelFactory.BuildComment(setRecipe: true, softDeleted: true);
        var audit = this.ModelFactory.BuildCommentAudit(comment, CommentAction.Delete);

        await this.DatabaseFixture.InsertEntities(currentUser, comment, audit);

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
            Audits = new[]
            {
                new
                {
                    audit.Id,
                    audit.Time,
                    Action = "DELETE",
                    Revision = default(object?),
                    Actor = IdName.From(audit.Actor),
                },
            },
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

    [Fact]
    public async Task QueryingIpAddressWhenAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        var comment = this.ModelFactory.BuildComment(setRecipe: true);
        var audit = this.ModelFactory.BuildCommentAudit(
            comment, CommentAction.Create, setOptionalAttributes: true);

        await this.DatabaseFixture.InsertEntities(currentUser, comment, audit);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostIpAddressQuery(client, comment.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var expected = new
        {
            Audits = new[]
            {
                new
                {
                    IpAddress = audit.IpAddress?.ToString(),
                },
            },
        };

        JsonAssert.Equivalent(expected, dataElement.GetProperty("comment"));
    }

    [Fact]
    public async Task QueryingIpAddressWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        var comment = this.ModelFactory.BuildComment(setRecipe: true);
        var audit = this.ModelFactory.BuildCommentAudit(
            comment, CommentAction.Create, setOptionalAttributes: true);

        await this.DatabaseFixture.InsertEntities(currentUser, comment, audit);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostIpAddressQuery(client, comment.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(
            document
                .RootElement
                .GetProperty("data")
                .GetProperty("comment")
                .GetProperty("audits")[0]
                .GetProperty("ipAddress"));

        var errorElement = ApiAssert.HasSingleError(
            ErrorCodes.Authentication.NotAuthorized, document);

        Assert.Collection(
            errorElement.GetProperty("path").EnumerateArray(),
            e => Assert.Equal("comment", e.GetString()),
            e => Assert.Equal("audits", e.GetString()),
            e => Assert.Equal(0, e.GetInt32()),
            e => Assert.Equal("ipAddress", e.GetString()));
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
                    audits {
                        id
                        time
                        action
                        revision { id body }
                        actor { id name }
                    }
                }
            }
            """,
            new { id });

    private static Task<HttpResponseMessage> PostIpAddressQuery(HttpClient client, long id) =>
        client.PostQuery("""
            query($id: Long!) {
                comment(id: $id) {
                    audits { ipAddress }
                }
            }
            """,
            new { id });
}
