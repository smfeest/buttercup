using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class AddFailureToUserAuditEntry : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder
            .AddColumn<string>(
                name: "failure",
                table: "user_audit_entries",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(
            name: "failure",
            table: "user_audit_entries");
}
