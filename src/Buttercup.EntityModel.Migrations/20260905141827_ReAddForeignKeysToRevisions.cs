using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class ReAddForeignKeysToRevisions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "ALTER TABLE recipe_revisions ADD recipe_id bigint AFTER id;");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_revisions_recipe_id",
            table: "recipe_revisions",
            column: "recipe_id");

        migrationBuilder.AddForeignKey(
            name: "fk_recipe_revisions_recipes_recipe_id",
            table: "recipe_revisions",
            column: "recipe_id",
            principalTable: "recipes",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.Sql(
            "ALTER TABLE comment_revisions ADD comment_id bigint AFTER id;");

        migrationBuilder.CreateIndex(
            name: "ix_comment_revisions_comment_id",
            table: "comment_revisions",
            column: "comment_id");

        migrationBuilder.AddForeignKey(
            name: "fk_comment_revisions_comments_comment_id",
            table: "comment_revisions",
            column: "comment_id",
            principalTable: "comments",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_recipe_revisions_recipes_recipe_id",
            table: "recipe_revisions");

        migrationBuilder.DropIndex(
            name: "ix_recipe_revisions_recipe_id",
            table: "recipe_revisions");

        migrationBuilder.DropColumn(
            name: "recipe_id",
            table: "recipe_revisions");

        migrationBuilder.DropForeignKey(
            name: "fk_comment_revisions_comments_comment_id",
            table: "comment_revisions");

        migrationBuilder.DropIndex(
            name: "ix_comment_revisions_comment_id",
            table: "comment_revisions");

        migrationBuilder.DropColumn(
            name: "comment_id",
            table: "comment_revisions");
    }
}
