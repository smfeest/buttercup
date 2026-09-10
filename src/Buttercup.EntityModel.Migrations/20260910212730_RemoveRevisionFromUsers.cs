using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class RemoveRevisionFromUsers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(
            name: "revision",
            table: "users");

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AddColumn<int>(
            name: "revision",
            table: "users",
            type: "int",
            nullable: false,
            defaultValue: 0);
}
