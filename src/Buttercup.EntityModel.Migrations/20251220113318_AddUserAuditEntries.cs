using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class AddUserAuditEntries : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "user_audit_entries",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                operation = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                target_id = table.Column<long>(type: "bigint", nullable: false),
                actor_id = table.Column<long>(type: "bigint", nullable: false),
                ip_address = table.Column<byte[]>(type: "varbinary(16)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_audit_entries", x => x.id);
                table.ForeignKey(
                    name: "fk_user_audit_entries_users_actor_id",
                    column: x => x.actor_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_user_audit_entries_users_target_id",
                    column: x => x.target_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "ix_user_audit_entries_actor_id",
            table: "user_audit_entries",
            column: "actor_id");

        migrationBuilder.CreateIndex(
            name: "ix_user_audit_entries_target_id",
            table: "user_audit_entries",
            column: "target_id");

        migrationBuilder.CreateIndex(
            name: "ix_user_audit_entries_time",
            table: "user_audit_entries",
            column: "time");
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "user_audit_entries");
}
