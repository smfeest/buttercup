using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class AddCommentsAndCommentRevisions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "comments",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                recipe_id = table.Column<long>(type: "bigint", nullable: false),
                author_id = table.Column<long>(type: "bigint", nullable: true),
                body = table.Column<string>(type: "text", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                modified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                deleted = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                deleted_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                revision = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_comments", x => x.id);
                table.ForeignKey(
                    name: "fk_comments_recipes_recipe_id",
                    column: x => x.recipe_id,
                    principalTable: "recipes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_comments_users_author_id",
                    column: x => x.author_id,
                    principalTable: "users",
                    principalColumn: "id");
                table.ForeignKey(
                    name: "fk_comments_users_deleted_by_user_id",
                    column: x => x.deleted_by_user_id,
                    principalTable: "users",
                    principalColumn: "id");
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "comment_revisions",
            columns: table => new
            {
                comment_id = table.Column<long>(type: "bigint", nullable: false),
                revision = table.Column<int>(type: "int", nullable: false),
                created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                body = table.Column<string>(type: "text", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_comment_revisions", x => new { x.comment_id, x.revision });
                table.ForeignKey(
                    name: "fk_comment_revisions_comments_comment_id",
                    column: x => x.comment_id,
                    principalTable: "comments",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "ix_comments_author_id",
            table: "comments",
            column: "author_id");

        migrationBuilder.CreateIndex(
            name: "ix_comments_deleted",
            table: "comments",
            column: "deleted");

        migrationBuilder.CreateIndex(
            name: "ix_comments_deleted_by_user_id",
            table: "comments",
            column: "deleted_by_user_id");

        migrationBuilder.CreateIndex(
            name: "ix_comments_recipe_id",
            table: "comments",
            column: "recipe_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("comment_revisions");
        migrationBuilder.DropTable("comments");
    }
}
