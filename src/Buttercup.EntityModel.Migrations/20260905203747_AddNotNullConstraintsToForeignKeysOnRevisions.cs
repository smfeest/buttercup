using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class AddNotNullConstraintsToForeignKeysOnRevisions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<long>(
            name: "recipe_id",
            table: "recipe_revisions",
            type: "bigint",
            nullable: false,
            defaultValue: 0L,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true);

        migrationBuilder.AlterColumn<long>(
            name: "comment_id",
            table: "comment_revisions",
            type: "bigint",
            nullable: false,
            defaultValue: 0L,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<long>(
            name: "recipe_id",
            table: "recipe_revisions",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint");

        migrationBuilder.AlterColumn<long>(
            name: "comment_id",
            table: "comment_revisions",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint");
    }
}
