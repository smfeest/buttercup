using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class AddRoleToUsers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder
            .AddColumn<string>(
                name: "role",
                table: "users",
                type: "varchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "contributor")
            .Annotation("MySql:CharSet", "utf8mb4");

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(
            name: "role",
            table: "users");
}
