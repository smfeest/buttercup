using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class DeletePasswordResetTokensWithoutHash : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("DELETE FROM password_reset_tokens WHERE hashed_token IS NULL;");

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
