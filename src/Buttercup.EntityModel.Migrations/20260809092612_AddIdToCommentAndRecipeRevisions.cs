using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class AddIdToCommentAndRecipeRevisions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "ix_recipe_revisions_recipe_id_revision",
            table: "recipe_revisions",
            columns: ["recipe_id", "revision"],
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_comment_revisions_comment_id_revision",
            table: "comment_revisions",
            columns: ["comment_id", "revision"],
            unique: true);

        migrationBuilder.DropPrimaryKey(
            name: "pk_recipe_revisions",
            table: "recipe_revisions");

        migrationBuilder.DropPrimaryKey(
            name: "pk_comment_revisions",
            table: "comment_revisions");

        migrationBuilder.Sql(
            "ALTER TABLE recipe_revisions ADD id bigint NOT NULL AUTO_INCREMENT PRIMARY KEY FIRST");

        migrationBuilder.Sql(
            "ALTER TABLE comment_revisions ADD id bigint NOT NULL AUTO_INCREMENT PRIMARY KEY FIRST");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "pk_recipe_revisions",
            table: "recipe_revisions");

        migrationBuilder.DropPrimaryKey(
            name: "pk_comment_revisions",
            table: "comment_revisions");

        migrationBuilder.DropColumn(
            name: "id",
            table: "recipe_revisions");

        migrationBuilder.DropColumn(
            name: "id",
            table: "comment_revisions");

        migrationBuilder.AddPrimaryKey(
            name: "pk_recipe_revisions",
            table: "recipe_revisions",
            columns: ["recipe_id", "revision"]);

        migrationBuilder.AddPrimaryKey(
            name: "pk_comment_revisions",
            table: "comment_revisions",
            columns: ["comment_id", "revision"]);

        migrationBuilder.DropIndex(
            name: "ix_recipe_revisions_recipe_id_revision",
            table: "recipe_revisions");

        migrationBuilder.DropIndex(
            name: "ix_comment_revisions_comment_id_revision",
            table: "comment_revisions");
    }
}
