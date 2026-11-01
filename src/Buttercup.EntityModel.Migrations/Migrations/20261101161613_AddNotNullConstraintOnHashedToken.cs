using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class AddNotNullConstraintOnHashedToken : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AlterColumn<byte[]>(
            name: "hashed_token",
            table: "password_reset_tokens",
            type: "bytea",
            nullable: false,
            oldClrType: typeof(byte[]),
            oldType: "bytea",
            oldNullable: true);

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AlterColumn<byte[]>(
            name: "hashed_token",
            table: "password_reset_tokens",
            type: "bytea",
            nullable: true,
            oldClrType: typeof(byte[]),
            oldType: "bytea");
}
