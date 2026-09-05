using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class BackfillForeignKeysAndDeleteOrphanedRevisions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        UPDATE recipe_revisions, recipe_audits
        SET recipe_revisions.recipe_id = recipe_audits.recipe_id
        WHERE recipe_audits.revision_id = recipe_revisions.id;

        DELETE recipe_revisions
        FROM recipe_revisions
        LEFT JOIN recipe_audits ON recipe_audits.revision_id = recipe_revisions.id
        WHERE recipe_audits.id IS NULL;

        UPDATE comment_revisions, comment_audits
        SET comment_revisions.comment_id = comment_audits.comment_id
        WHERE comment_audits.revision_id = comment_revisions.id;

        DELETE comment_revisions
        FROM comment_revisions
        LEFT JOIN comment_audits ON comment_audits.revision_id = comment_revisions.id
        WHERE comment_audits.id IS NULL;
        """);
}
