using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class AddDeletedAndDeletedByUserToRecipe : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "deleted",
            table: "recipes",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<long>(
            name: "deleted_by_user_id",
            table: "recipes",
            type: "bigint",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_recipes_deleted",
            table: "recipes",
            column: "deleted");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_deleted_by_user_id",
            table: "recipes",
            column: "deleted_by_user_id");

        migrationBuilder.AddForeignKey(
            name: "fk_recipes_users_deleted_by_user_id",
            table: "recipes",
            column: "deleted_by_user_id",
            principalTable: "users",
            principalColumn: "id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_recipes_users_deleted_by_user_id",
            table: "recipes");

        migrationBuilder.DropIndex(
            name: "ix_recipes_deleted",
            table: "recipes");

        migrationBuilder.DropIndex(
            name: "ix_recipes_deleted_by_user_id",
            table: "recipes");

        migrationBuilder.DropColumn(
            name: "deleted",
            table: "recipes");

        migrationBuilder.DropColumn(
            name: "deleted_by_user_id",
            table: "recipes");
    }
}
