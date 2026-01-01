using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class IncreaseMaxLengthOfOperationOnUserAuditEntry : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AlterColumn<string>(
            name: "operation",
            table: "user_audit_entries",
            type: "varchar(30)",
            maxLength: 30,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(15)",
            oldMaxLength: 15)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AlterColumn<string>(
            name: "operation",
            table: "user_audit_entries",
            type: "varchar(15)",
            maxLength: 15,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(30)",
            oldMaxLength: 30)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");
}
