using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class BackfillRoleForExistingAdministrators : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("UPDATE users SET role = 'admin' WHERE is_admin = 1");

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
