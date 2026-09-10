using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class SetDefaultForRevisionOnUsers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AlterColumn<int>(
            name: "revision",
            table: "users",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int");

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AlterColumn<int>(
            name: "revision",
            table: "users",
            type: "int",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int",
            oldDefaultValue: 0);
}
