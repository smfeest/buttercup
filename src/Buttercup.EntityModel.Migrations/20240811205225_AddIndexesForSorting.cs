using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class AddIndexesForSorting : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "ix_users_created",
            table: "users",
            column: "created");

        migrationBuilder.CreateIndex(
            name: "ix_users_modified",
            table: "users",
            column: "modified");

        migrationBuilder.CreateIndex(
            name: "ix_users_name",
            table: "users",
            column: "name");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_created",
            table: "recipes",
            column: "created");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_modified",
            table: "recipes",
            column: "modified");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_title",
            table: "recipes",
            column: "title");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_users_created",
            table: "users");

        migrationBuilder.DropIndex(
            name: "ix_users_modified",
            table: "users");

        migrationBuilder.DropIndex(
            name: "ix_users_name",
            table: "users");

        migrationBuilder.DropIndex(
            name: "ix_recipes_created",
            table: "recipes");

        migrationBuilder.DropIndex(
            name: "ix_recipes_modified",
            table: "recipes");

        migrationBuilder.DropIndex(
            name: "ix_recipes_title",
            table: "recipes");
    }
}
