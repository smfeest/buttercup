using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class DropSecurityEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(
            name: "security_events");

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "security_events",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                user_id = table.Column<long>(type: "bigint", nullable: true),
                @event = table.Column<string>(name: "event", type: "varchar(50)", maxLength: 50, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ip_address = table.Column<byte[]>(type: "varbinary(16)", nullable: true),
                time = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_security_events", x => x.id);
                table.ForeignKey(
                    name: "fk_security_events_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id");
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "ix_security_events_user_id",
            table: "security_events",
            column: "user_id");
    }
}
