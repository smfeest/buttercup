using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class DropTokenFromPasswordResetTokens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_password_reset_tokens_token",
            table: "password_reset_tokens");

        migrationBuilder.DropColumn(
            name: "token",
            table: "password_reset_tokens");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "token",
            table: "password_reset_tokens",
            type: "character varying(48)",
            maxLength: 48,
            nullable: true,
            collation: "C");

        migrationBuilder.CreateIndex(
            name: "ix_password_reset_tokens_token",
            table: "password_reset_tokens",
            column: "token",
            unique: true);
    }
}
