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

    #region CreateComment

    [Fact]
    public async Task CreateComment_InsertsCommentAuditAndRevisionAndReturnsId()
    {
        var recipe = this.modelFactory.BuildRecipe(setOptionalAttributes: true);
        var currentUser = this.modelFactory.BuildUser();
        var ipAddress = this.modelFactory.NextIpAddress();

        await this.DatabaseFixture.InsertEntities(recipe, currentUser);

        var attributes = this.BuildCommentAttributes();
        var id = await this.commentManager.CreateComment(
            recipe.Id,
            attributes,
            currentUser.Id,
            ipAddress,
            TestContext.Current.CancellationToken);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        var comment = await dbContext
            .Comments
            .Include(c => c.Audits)
            .ThenInclude(a => a.Revision)
            .GetAsync(id, TestContext.Current.CancellationToken);

        var expectedTimestamp = this.timeProvider.GetUtcDateTimeNow();

        Assert.Equal(
            new()
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
            },
            comment,
            ModelCompare.EqualExcludingNavigationProperties);

        var audit = Assert.Single(comment.Audits);

        Assert.Equal(
            new()
            {
                Id = audit.Id,
                CommentId = id,
                Time = this.timeProvider.GetUtcDateTimeNow(),
                Action = CommentAction.Create,
                RevisionId = audit.RevisionId,
                ActorId = currentUser.Id,
                IpAddress = ipAddress,
            },
            audit,
            ModelCompare.EqualExcludingNavigationProperties);

        Assert.NotNull(audit.Revision);

        Assert.Equal(attributes.Body, audit.Revision.Body);
    }

    [Fact]
    public async Task CreateComment_ThrowsIfRecipeNotFound()
    {
        var otherRecipe = this.modelFactory.BuildRecipe();
        var currentUser = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(otherRecipe, currentUser);

        var recipeId = this.modelFactory.NextInt();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.commentManager.CreateComment(
                recipeId,
                this.BuildCommentAttributes(),
                currentUser.Id,
                this.modelFactory.NextIpAddress(),
                TestContext.Current.CancellationToken));

        Assert.Equal($"Recipe/{recipeId} not found", exception.Message);
    }

    [Fact]
    public async Task CreateComment_ThrowsIfRecipeSoftDeleted()
    {
        var recipe = this.modelFactory.BuildRecipe(softDeleted: true);
        var currentUser = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(recipe, currentUser);

        var exception = await Assert.ThrowsAsync<SoftDeletedException>(
            () => this.commentManager.CreateComment(
                recipe.Id,
                this.BuildCommentAttributes(),
                currentUser.Id,
                this.modelFactory.NextIpAddress(),
                TestContext.Current.CancellationToken));

        Assert.Equal($"Cannot add comment to soft-deleted recipe {recipe.Id}", exception.Message);
    }

    #endregion

    #region DeleteComment

    [Fact]
    public async Task DeleteComment_SetsSoftDeleteAttributesInsertsAuditAndReturnsTrue()
    {
        var original = this.modelFactory.BuildComment(setRecipe: true, softDeleted: false);
        var currentUser = this.modelFactory.BuildUser();
        var ipAddress = this.modelFactory.NextIpAddress();

        await this.DatabaseFixture.InsertEntities(original, currentUser);

        Assert.True(await this.commentManager.DeleteComment(
            original.Id, currentUser.Id, ipAddress, TestContext.Current.CancellationToken));

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        var comment = await dbContext
            .Comments
            .Include(c => c.Audits)
            .GetAsync(original.Id, TestContext.Current.CancellationToken);

        Assert.Equal(
            original with
            {
                Deleted = this.timeProvider.GetUtcDateTimeNow(),
                DeletedByUserId = currentUser.Id,
            },
            comment,
            ModelCompare.EqualExcludingNavigationProperties);

        var audit = Assert.Single(comment.Audits);

        Assert.Equal(
            new CommentAudit()
            {
                Id = audit.Id,
                CommentId = comment.Id,
                Time = this.timeProvider.GetUtcDateTimeNow(),
                Action = CommentAction.Delete,
                RevisionId = null,
                ActorId = currentUser.Id,
                IpAddress = ipAddress,
            },
            audit,
            ModelCompare.EqualExcludingNavigationProperties);
    }

    [Fact]
    public async Task DeleteComment_DoesNotUpdateCommentOrInsertAuditAndReturnsFalseIfAlreadySoftDeleted()
    {
        var original = this.modelFactory.BuildComment(setRecipe: true, softDeleted: true);
        var currentUser = this.modelFactory.BuildUser();
        var ipAddress = this.modelFactory.NextIpAddress();

        await this.DatabaseFixture.InsertEntities(original, currentUser);

        Assert.False(await this.commentManager.DeleteComment(
            original.Id, currentUser.Id, ipAddress, TestContext.Current.CancellationToken));

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        var comment = await dbContext
            .Comments
            .Include(r => r.Audits)
            .GetAsync(original.Id, TestContext.Current.CancellationToken);

        Assert.Equal(original, comment, ModelCompare.EqualExcludingNavigationProperties);
        Assert.Empty(comment.Audits);
    }

    [Fact]
    public async Task DeleteComment_ReturnsFalseIfRecordNotFound()
    {
        var currentUser = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(
            this.modelFactory.BuildComment(setRecipe: true), currentUser);

        Assert.False(
            await this.commentManager.DeleteComment(
                this.modelFactory.NextInt(),
                currentUser.Id,
                this.modelFactory.NextIpAddress(),
                TestContext.Current.CancellationToken));
    }

    #endregion

    #region HardDeleteComment

    [Fact]
    public async Task HardDeleteComment_HardDeletesCommentAndReturnsTrue()
    {
        var comment = this.modelFactory.BuildComment(setRecipe: true);
        await this.DatabaseFixture.InsertEntities(comment);

        Assert.True(await this.commentManager.HardDeleteComment(
            comment.Id, TestContext.Current.CancellationToken));

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        Assert.False(await dbContext.Comments.AnyAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HardDeleteComment_ReturnsFalseIfRecordNotFound()
    {
        await this.DatabaseFixture.InsertEntities(this.modelFactory.BuildComment(setRecipe: true));

        Assert.False(await this.commentManager.HardDeleteComment(
            this.modelFactory.NextInt(), TestContext.Current.CancellationToken));
    }

    #endregion

    private CommentAttributes BuildCommentAttributes() =>
        new() { Body = this.modelFactory.NextString("comment-body") };
}
