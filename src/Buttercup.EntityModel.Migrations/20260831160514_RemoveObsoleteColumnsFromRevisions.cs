using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class RemoveObsoleteColumnsFromRevisions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_comment_revisions_comments_comment_id",
            table: "comment_revisions");

        migrationBuilder.DropForeignKey(
            name: "fk_recipe_revisions_recipes_recipe_id",
            table: "recipe_revisions");

        migrationBuilder.DropForeignKey(
            name: "fk_recipe_revisions_users_created_by_user_id",
            table: "recipe_revisions");

        migrationBuilder.DropIndex(
            name: "ix_recipe_revisions_created_by_user_id",
            table: "recipe_revisions");

        migrationBuilder.DropIndex(
            name: "ix_recipe_revisions_recipe_id",
            table: "recipe_revisions");

        migrationBuilder.DropIndex(
            name: "ix_comment_revisions_comment_id",
            table: "comment_revisions");

        migrationBuilder.DropColumn(
            name: "created",
            table: "recipe_revisions");

        migrationBuilder.DropColumn(
            name: "created_by_user_id",
            table: "recipe_revisions");

        migrationBuilder.DropColumn(
            name: "recipe_id",
            table: "recipe_revisions");

        migrationBuilder.DropColumn(
            name: "revision",
            table: "recipe_revisions");

        migrationBuilder.DropColumn(
            name: "comment_id",
            table: "comment_revisions");

        migrationBuilder.DropColumn(
            name: "created",
            table: "comment_revisions");

        migrationBuilder.DropColumn(
            name: "revision",
            table: "comment_revisions");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "created",
            table: "recipe_revisions",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<long>(
            name: "created_by_user_id",
            table: "recipe_revisions",
            type: "bigint",
            nullable: true);

        migrationBuilder.AddColumn<long>(
            name: "recipe_id",
            table: "recipe_revisions",
            type: "bigint",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "revision",
            table: "recipe_revisions",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<long>(
            name: "comment_id",
            table: "comment_revisions",
            type: "bigint",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "created",
            table: "comment_revisions",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "revision",
            table: "comment_revisions",
            type: "int",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_recipe_revisions_created_by_user_id",
            table: "recipe_revisions",
            column: "created_by_user_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_revisions_recipe_id",
            table: "recipe_revisions",
            column: "recipe_id");

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

        migrationBuilder.AddForeignKey(
            name: "fk_recipe_revisions_recipes_recipe_id",
            table: "recipe_revisions",
            column: "recipe_id",
            principalTable: "recipes",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_recipe_revisions_users_created_by_user_id",
            table: "recipe_revisions",
            column: "created_by_user_id",
            principalTable: "users",
            principalColumn: "id");
    }
}
