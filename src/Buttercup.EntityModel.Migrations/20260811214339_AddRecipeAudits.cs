using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class AddRecipeAudits : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "recipe_audits",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                recipe_id = table.Column<long>(type: "bigint", nullable: false),
                time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                action = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                revision_id = table.Column<long>(type: "bigint", nullable: true),
                actor_id = table.Column<long>(type: "bigint", nullable: true),
                ip_address = table.Column<byte[]>(type: "varbinary(16)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipe_audits", x => x.id);
                table.ForeignKey(
                    name: "fk_recipe_audits_recipe_revisions_revision_id",
                    column: x => x.revision_id,
                    principalTable: "recipe_revisions",
                    principalColumn: "id");
                table.ForeignKey(
                    name: "fk_recipe_audits_recipes_recipe_id",
                    column: x => x.recipe_id,
                    principalTable: "recipes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_recipe_audits_users_actor_id",
                    column: x => x.actor_id,
                    principalTable: "users",
                    principalColumn: "id");
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_audits_actor_id",
            table: "recipe_audits",
            column: "actor_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_audits_recipe_id",
            table: "recipe_audits",
            column: "recipe_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_audits_revision_id",
            table: "recipe_audits",
            column: "revision_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "recipe_audits");
}
