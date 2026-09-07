using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class RemoveRevisionFromRecipesAndComments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "revision",
            table: "recipes");

        migrationBuilder.DropColumn(
            name: "revision",
            table: "comments");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "revision",
            table: "recipes",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "revision",
            table: "comments",
            type: "int",
            nullable: false,
            defaultValue: 0);
    }
}
