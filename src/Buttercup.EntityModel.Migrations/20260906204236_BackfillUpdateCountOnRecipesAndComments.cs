using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class BackfillUpdateCountOnRecipesAndComments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        UPDATE recipes
        SET update_count = IF(deleted IS NULL, revision, revision + 1);

        UPDATE comments
        SET update_count = IF(deleted IS NULL, revision, revision + 1);
        """);

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
