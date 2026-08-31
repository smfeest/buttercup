using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class RemoveConstraintsFromObsoleteColumnsOnRevisions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "ix_recipe_revisions_recipe_id",
            table: "recipe_revisions",
            column: "recipe_id");

        migrationBuilder.CreateIndex(
            name: "ix_comment_revisions_comment_id",
            table: "comment_revisions",
            column: "comment_id");

        migrationBuilder.DropIndex(
            name: "ix_recipe_revisions_recipe_id_revision",
            table: "recipe_revisions");

        migrationBuilder.DropIndex(
            name: "ix_comment_revisions_comment_id_revision",
            table: "comment_revisions");

        migrationBuilder.AlterColumn<int>(
            name: "revision",
            table: "recipe_revisions",
            type: "int",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.AlterColumn<long>(
            name: "recipe_id",
            table: "recipe_revisions",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint");

        migrationBuilder.AlterColumn<DateTime>(
            name: "created",
            table: "recipe_revisions",
            type: "datetime(6)",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime(6)");

        migrationBuilder.AlterColumn<int>(
            name: "revision",
            table: "comment_revisions",
            type: "int",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.AlterColumn<DateTime>(
            name: "created",
            table: "comment_revisions",
            type: "datetime(6)",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime(6)");

        migrationBuilder.AlterColumn<long>(
            name: "comment_id",
            table: "comment_revisions",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "ix_recipe_revisions_recipe_id_revision",
            table: "recipe_revisions",
            columns: ["recipe_id", "revision"],
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_comment_revisions_comment_id_revision",
            table: "comment_revisions",
            columns: ["comment_id", "revision"],
            unique: true);

        migrationBuilder.DropIndex(
            name: "ix_recipe_revisions_recipe_id",
            table: "recipe_revisions");

        migrationBuilder.DropIndex(
            name: "ix_comment_revisions_comment_id",
            table: "comment_revisions");

        migrationBuilder.AlterColumn<int>(
            name: "revision",
            table: "recipe_revisions",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AlterColumn<long>(
            name: "recipe_id",
            table: "recipe_revisions",
            type: "bigint",
            nullable: false,
            defaultValue: 0L,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTime>(
            name: "created",
            table: "recipe_revisions",
            type: "datetime(6)",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
            oldClrType: typeof(DateTime),
            oldType: "datetime(6)",
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "revision",
            table: "comment_revisions",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTime>(
            name: "created",
            table: "comment_revisions",
            type: "datetime(6)",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
            oldClrType: typeof(DateTime),
            oldType: "datetime(6)",
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
}
