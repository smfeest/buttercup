using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class SetBinaryCollationOnPasswordResetTokens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AlterColumn<string>(
            name: "token",
            table: "password_reset_tokens",
            type: "char(48)",
            maxLength: 48,
            nullable: false,
            collation: "ascii_bin",
            oldClrType: typeof(string),
            oldType: "char(48)",
            oldMaxLength: 48)
            .OldAnnotation("MySql:CharSet", "utf8mb4");

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AlterColumn<string>(
            name: "token",
            table: "password_reset_tokens",
            type: "char(48)",
            maxLength: 48,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "char(48)",
            oldMaxLength: 48,
            oldCollation: "ascii_bin")
            .Annotation("MySql:CharSet", "utf8mb4");
}
