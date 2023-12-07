using Microsoft.EntityFrameworkCore.Migrations;


namespace Buttercup.EntityModel.Migrations;

public partial class AddIsAdminToUser : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AddColumn<bool>(
            name: "is_admin",
            table: "users",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(
            name: "is_admin",
            table: "users");
}
