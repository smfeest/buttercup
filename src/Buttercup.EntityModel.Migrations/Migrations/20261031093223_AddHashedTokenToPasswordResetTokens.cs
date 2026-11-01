using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class AddHashedTokenToPasswordResetTokens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<byte[]>(
            name: "hashed_token",
            table: "password_reset_tokens",
            type: "bytea",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_password_reset_tokens_hashed_token",
            table: "password_reset_tokens",
            column: "hashed_token",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_password_reset_tokens_hashed_token",
            table: "password_reset_tokens");

        migrationBuilder.DropColumn(
            name: "hashed_token",
            table: "password_reset_tokens");
    }
}
