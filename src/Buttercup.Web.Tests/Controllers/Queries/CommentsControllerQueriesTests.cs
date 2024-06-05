using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.Controllers.Queries;

[Collection(nameof(DatabaseCollection))]
public sealed class CommentsControllerQueriesTests(
    DatabaseFixture<DatabaseCollection> databaseFixture)
    : DatabaseTests<DatabaseCollection>(databaseFixture)
{
    private readonly ModelFactory modelFactory = new();
    private readonly CommentsControllerQueries queries = new();

    #region FindComment

    [Fact]
    public async Task FindComment_ReturnsCommentIfExistsAndNotDeleted()
    {
        var accessibleComment = this.modelFactory.BuildComment(
            setOptionalAttributes: true, setRecipe: true);
        var softDeletedComment = this.modelFactory.BuildComment(
            setOptionalAttributes: true, setRecipe: true, softDeleted: true);
        await this.DatabaseFixture.InsertEntities(accessibleComment, softDeletedComment);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        Assert.Equivalent(
            accessibleComment with { Recipe = null, Author = null },
            await this.queries.FindComment(dbContext, accessibleComment.Id));
        Assert.Null(
            await this.queries.FindComment(dbContext, softDeletedComment.Id));
        Assert.Null(
            await this.queries.FindComment(dbContext, this.modelFactory.NextInt()));
    }

    #endregion

    #region FindCommentWithAuthor

    [Fact]
    public async Task FindCommentWithAuthor_ReturnsCommentWithAuthorIfExistsAndNotDeleted()
    {
        var accessibleComment = this.modelFactory.BuildComment(
            setOptionalAttributes: true, setRecipe: true);
        var softDeletedComment = this.modelFactory.BuildComment(
            setOptionalAttributes: true, setRecipe: true, softDeleted: true);
        await this.DatabaseFixture.InsertEntities(accessibleComment, softDeletedComment);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        Assert.Equivalent(
            accessibleComment with { Recipe = null },
            await this.queries.FindCommentWithAuthor(dbContext, accessibleComment.Id));
        Assert.Null(
            await this.queries.FindCommentWithAuthor(dbContext, softDeletedComment.Id));
        Assert.Null(
            await this.queries.FindCommentWithAuthor(dbContext, this.modelFactory.NextInt()));
    }

    #endregion
}
