using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class MakeUserIdPrimaryKeyOnPasswordResetTokens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM password_reset_tokens
            WHERE token NOT IN (
                SELECT DISTINCT ON (user_id) token
                FROM password_reset_tokens
                ORDER BY user_id, created DESC);
            """);

        migrationBuilder.DropPrimaryKey(
            name: "pk_password_reset_tokens",
            table: "password_reset_tokens");

        migrationBuilder.DropIndex(
            name: "ix_password_reset_tokens_user_id",
            table: "password_reset_tokens");

        migrationBuilder.AddPrimaryKey(
            name: "pk_password_reset_tokens",
            table: "password_reset_tokens",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_password_reset_tokens_token",
            table: "password_reset_tokens",
            column: "token",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "pk_password_reset_tokens",
            table: "password_reset_tokens");

        migrationBuilder.DropIndex(
            name: "ix_password_reset_tokens_token",
            table: "password_reset_tokens");

        migrationBuilder.AddPrimaryKey(
            name: "pk_password_reset_tokens",
            table: "password_reset_tokens",
            column: "token");

        migrationBuilder.CreateIndex(
            name: "ix_password_reset_tokens_user_id",
            table: "password_reset_tokens",
            column: "user_id");
    }
}
