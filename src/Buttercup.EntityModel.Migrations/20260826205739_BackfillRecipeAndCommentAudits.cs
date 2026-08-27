using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class BackfillRecipeAndCommentAudits : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        INSERT INTO recipe_audits (recipe_id, time, action, revision_id, actor_id)
        SELECT
            recipes.id,
            recipe_revisions.created,
            IF(recipe_revisions.revision = 0 AND recipe_revisions.created = recipes.created, 'create', 'update'),
            recipe_revisions.id,
            recipe_revisions.created_by_user_id
        FROM recipe_revisions
        INNER JOIN recipes ON recipes.id = recipe_revisions.recipe_id
        LEFT JOIN recipe_audits ON recipe_audits.revision_id = recipe_revisions.id
        WHERE recipe_audits.id IS NULL;

        INSERT INTO recipe_audits (recipe_id, time, action, actor_id)
        SELECT recipes.id, recipes.deleted, 'delete', recipes.deleted_by_user_id
        FROM recipes
        LEFT JOIN recipe_audits
            ON recipe_audits.recipe_id = recipes.id AND recipe_audits.action = 'delete'
        WHERE recipes.deleted IS NOT NULL AND recipe_audits.id IS NULL;

        INSERT INTO comment_audits (comment_id, time, action, revision_id, actor_id)
        SELECT
            comments.id,
            comment_revisions.created,
            'create',
            comment_revisions.id,
            comments.author_id
        FROM comment_revisions
        INNER JOIN comments ON comments.id = comment_revisions.comment_id
        LEFT JOIN comment_audits ON comment_audits.revision_id = comment_revisions.id
        WHERE comment_audits.id IS NULL;

        INSERT INTO comment_audits (comment_id, time, action, actor_id)
        SELECT comments.id, comments.deleted, 'delete', comments.deleted_by_user_id
        FROM comments
        LEFT JOIN comment_audits
            ON comment_audits.comment_id = comments.id AND comment_audits.action = 'delete'
        WHERE comments.deleted IS NOT NULL AND comment_audits.id IS NULL;
        """);

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
