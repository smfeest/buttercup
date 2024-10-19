using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class CommentsTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    private const string CommentsQuery = """
        query {
            comments {
                nodes {
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

        JsonAssert.Equivalent(expected, dataElement.GetProperty("comments").GetProperty("nodes"));
    }

    [Fact]
    public async Task QueryingCommentsWhenUnauthenticated()
    {
        using var client = this.AppFactory.CreateClient();
        using var response = await client.PostQuery(CommentsQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data").GetProperty("comments"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }

    [Fact]
    public async Task FilteringComments()
    {
        var currentUser = this.ModelFactory.BuildUser();
        var recipe = this.ModelFactory.BuildRecipe();
        var comments = new[]
        {
            this.ModelFactory.BuildComment() with { Recipe = recipe, Body = "Delectable" },
            this.ModelFactory.BuildComment() with { Recipe = recipe, Body = "Palatable" },
        };
        await this.DatabaseFixture.InsertEntities(currentUser, comments);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery("""
            query {
                comments(where: { body: { startsWith: "Pal" } }) {
                    nodes {
                        body
                    }
                }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);
        var nodesArray = dataElement.GetProperty("comments").GetProperty("nodes").EnumerateArray();
        Assert.Equal("Palatable", Assert.Single(nodesArray).GetProperty("body").GetString());
    }

    [Fact]
    public async Task FilteringCommentsByAdminOnlyUserFieldWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery("""
            query {
                comments(where: { author: { email: { eq: "foo@example.com" } } }) {
                    nodes {
                        id
                    }
                }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data").GetProperty("comments"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }

    [Fact]
    public async Task SortingAndPagingComments()
    {
        var currentUser = this.ModelFactory.BuildUser();
        var recipe = this.ModelFactory.BuildRecipe();
        var baseCreated = this.ModelFactory.NextDateTime();
        var comments = new[]
        {
            this.ModelFactory.BuildComment() with
                { Id = 1, Recipe = recipe, Created = baseCreated },
            this.ModelFactory.BuildComment() with
                { Id = 2, Recipe = recipe, Created = baseCreated.AddHours(10) },
            this.ModelFactory.BuildComment() with
                { Id = 3, Recipe = recipe, Created = baseCreated.AddHours(5) },
            this.ModelFactory.BuildComment() with
                { Id = 4, Recipe = recipe, Created = baseCreated.AddHours(15) },
        };
        await this.DatabaseFixture.InsertEntities(currentUser, comments);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);

        Task<HttpResponseMessage> PostQuery(string? afterCursor) => client.PostQuery("""
            query($afterCursor: String) {
                comments(first: 2, after: $afterCursor, order: { created: ASC }) {
                    nodes {
                        id
                    }
                    pageInfo {
                        endCursor
                    }
                }
            }
            """,
            new { afterCursor });

        string? endCursor;

        using (var response = await PostQuery(null))
        {
            using var document = await response.Content.ReadAsJsonDocument();

            var dataElement = ApiAssert.SuccessResponse(document);
            var commentsElement = dataElement.GetProperty("comments");

            Assert.Equal(
                [1, 3],
                commentsElement
                    .GetProperty("nodes")
                    .EnumerateArray().Select(c => c.GetProperty("id").GetInt64()));

            endCursor = commentsElement
                .GetProperty("pageInfo")
                .GetProperty("endCursor")
                .GetString();
        }

        using (var response = await PostQuery(endCursor))
        {
            using var document = await response.Content.ReadAsJsonDocument();

            var dataElement = ApiAssert.SuccessResponse(document);
            Assert.Equal(
                [2, 4],
                dataElement
                    .GetProperty("comments")
                    .GetProperty("nodes")
                    .EnumerateArray().Select(c => c.GetProperty("id").GetInt64()));
        }
    }

    [Fact]
    public async Task SortingCommentsByAdminOnlyUserFieldsWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery("""
            query {
                comments(order: { author: { email: ASC } }) {
                    nodes {
                        id
                    }
                }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data").GetProperty("comments"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }
}
