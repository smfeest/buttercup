using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class RenameAuthenticationEventToSecurityEvent : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameTable(name: "authentication_events", newName: "security_events");
        migrationBuilder.RenameIndex(
            "ix_authentication_events_user_id", "ix_security_events_user_id", "security_events");
        migrationBuilder.DropForeignKey(
            "fk_authentication_events_users_user_id", "security_events");
        migrationBuilder.AddForeignKey(
            name: "fk_security_events_users_user_id",
            table: "security_events",
            column: "user_id",
            principalTable: "users",
            principalColumn: "id");
    }
}

