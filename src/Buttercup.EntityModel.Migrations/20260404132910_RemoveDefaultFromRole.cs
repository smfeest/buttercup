using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class RemoveDefaultFromRole : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder
            .AlterColumn<string>(
                name: "role",
                table: "users",
                type: "varchar(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)",
                oldMaxLength: 15,
                oldDefaultValue: "contributor")
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder
            .AlterColumn<string>(
                name: "role",
                table: "users",
                type: "varchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "contributor",
                oldClrType: typeof(string),
                oldType: "varchar(15)",
                oldMaxLength: 15)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");
}
