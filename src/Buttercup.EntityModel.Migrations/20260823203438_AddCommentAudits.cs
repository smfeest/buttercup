using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class AddCommentAudits : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "comment_audits",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                comment_id = table.Column<long>(type: "bigint", nullable: false),
                time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                action = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                revision_id = table.Column<long>(type: "bigint", nullable: true),
                actor_id = table.Column<long>(type: "bigint", nullable: true),
                ip_address = table.Column<byte[]>(type: "varbinary(16)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_comment_audits", x => x.id);
                table.ForeignKey(
                    name: "fk_comment_audits_comment_revisions_revision_id",
                    column: x => x.revision_id,
                    principalTable: "comment_revisions",
                    principalColumn: "id");
                table.ForeignKey(
                    name: "fk_comment_audits_comments_comment_id",
                    column: x => x.comment_id,
                    principalTable: "comments",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_comment_audits_users_actor_id",
                    column: x => x.actor_id,
                    principalTable: "users",
                    principalColumn: "id");
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "ix_comment_audits_actor_id",
            table: "comment_audits",
            column: "actor_id");

        migrationBuilder.CreateIndex(
            name: "ix_comment_audits_comment_id",
            table: "comment_audits",
            column: "comment_id");

        migrationBuilder.CreateIndex(
            name: "ix_comment_audits_revision_id",
            table: "comment_audits",
            column: "revision_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "comment_audits");
}
