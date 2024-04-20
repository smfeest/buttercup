using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace Buttercup.Application;

[Collection(nameof(DatabaseCollection))]
public sealed class CommentManagerTests : DatabaseTests<DatabaseCollection>
{
    private readonly ModelFactory modelFactory = new();

    private readonly FakeTimeProvider timeProvider;
    private readonly CommentManager commentManager;

    public CommentManagerTests(DatabaseFixture<DatabaseCollection> databaseFixture)
        : base(databaseFixture)
    {
        this.timeProvider = new(this.modelFactory.NextDateTime());
        this.commentManager = new(databaseFixture, this.timeProvider);
    }

    #region AddComment

    [Fact]
    public async Task AddComment_InsertsCommentAndRevisionAndReturnsId()
    {
        var recipe = this.modelFactory.BuildRecipe(setOptionalAttributes: true);
        var currentUser = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(recipe, currentUser);

        var attributes = this.BuildCommentAttributes();
        var id = await this.commentManager.AddComment(recipe.Id, attributes, currentUser.Id);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        var expectedTimestamp = this.timeProvider.GetUtcDateTimeNow();
        var expectedComment = new Comment
        {
            Id = id,
            RecipeId = recipe.Id,
            AuthorId = currentUser.Id,
            Body = attributes.Body,
            Created = expectedTimestamp,
            Modified = expectedTimestamp,
            Deleted = null,
            DeletedByUserId = null,
            Revision = 0,
        };
        var actualComment = await dbContext.Comments.FindAsync(id);
        Assert.Equivalent(expectedComment, actualComment);

        var expectedRevision = new CommentRevision
        {
            CommentId = id,
            Revision = 0,
            Created = expectedTimestamp,
            Body = attributes.Body,
        };
        var actualRevision = await dbContext
            .CommentRevisions
            .Where(r => r.CommentId == id)
            .SingleAsync();
        Assert.Equal(expectedRevision, actualRevision);
    }

    [Fact]
    public async Task AddComment_ThrowsIfRecipeNotFound()
    {
        var otherRecipe = this.modelFactory.BuildRecipe();
        var currentUser = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(otherRecipe, currentUser);

        var recipeId = this.modelFactory.NextInt();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.commentManager.AddComment(
                recipeId, this.BuildCommentAttributes(), currentUser.Id));

        Assert.Equal($"Recipe/{recipeId} not found", exception.Message);
    }

    [Fact]
    public async Task AddComment_ThrowsIfRecipeSoftDeleted()
    {
        var recipe = this.modelFactory.BuildRecipe(softDeleted: true);
        var currentUser = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(recipe, currentUser);

        var exception = await Assert.ThrowsAsync<SoftDeletedException>(
            () => this.commentManager.AddComment(
                recipe.Id, this.BuildCommentAttributes(), currentUser.Id));

        Assert.Equal($"Cannot add comment to soft-deleted recipe {recipe.Id}", exception.Message);
    }

    #endregion

    private CommentAttributes BuildCommentAttributes() =>
        new() { Body = this.modelFactory.NextString("comment-body") };
}
