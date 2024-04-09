using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class AddRecipeRevisions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "recipe_revisions",
            columns: table => new
            {
                recipe_id = table.Column<long>(type: "bigint", nullable: false),
                revision = table.Column<int>(type: "int", nullable: false),
                created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                created_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                title = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                preparation_minutes = table.Column<int>(type: "int", nullable: true),
                cooking_minutes = table.Column<int>(type: "int", nullable: true),
                servings = table.Column<int>(type: "int", nullable: true),
                ingredients = table.Column<string>(type: "text", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                method = table.Column<string>(type: "text", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                suggestions = table.Column<string>(type: "text", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                remarks = table.Column<string>(type: "text", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                source = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipe_revisions", x => new { x.recipe_id, x.revision });
                table.ForeignKey(
                    name: "fk_recipe_revisions_recipes_recipe_id",
                    column: x => x.recipe_id,
                    principalTable: "recipes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_recipe_revisions_users_created_by_user_id",
                    column: x => x.created_by_user_id,
                    principalTable: "users",
                    principalColumn: "id");
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "ix_recipe_revisions_created_by_user_id",
            table: "recipe_revisions",
            column: "created_by_user_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "recipe_revisions");
}
