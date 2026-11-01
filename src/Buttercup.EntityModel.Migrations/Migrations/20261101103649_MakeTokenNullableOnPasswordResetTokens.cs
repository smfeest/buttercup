using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class MakeTokenNullableOnPasswordResetTokens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AlterColumn<string>(
            name: "token",
            table: "password_reset_tokens",
            type: "character varying(48)",
            maxLength: 48,
            nullable: true,
            collation: "C",
            oldClrType: typeof(string),
            oldType: "character varying(48)",
            oldMaxLength: 48,
            oldCollation: "C");

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AlterColumn<string>(
            name: "token",
            table: "password_reset_tokens",
            type: "character varying(48)",
            maxLength: 48,
            nullable: false,
            defaultValue: "",
            collation: "C",
            oldClrType: typeof(string),
            oldType: "character varying(48)",
            oldMaxLength: 48,
            oldNullable: true,
            oldCollation: "C");
}
