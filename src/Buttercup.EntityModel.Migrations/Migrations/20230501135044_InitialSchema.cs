using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations.Migrations;

public partial class InitialSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                name = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                email = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                hashed_password = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                password_created = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                security_stamp = table.Column<string>(type: "char(8)", maxLength: 8, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                time_zone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                modified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                revision = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
                table.UniqueConstraint("ak_users_email", x => x.email);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "authentication_events",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                @event = table.Column<string>(name: "event", type: "varchar(50)", maxLength: 50, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                user_id = table.Column<long>(type: "bigint", nullable: true),
                email = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_authentication_events", x => x.id);
                table.ForeignKey(
                    name: "fk_authentication_events_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id");
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "password_reset_tokens",
            columns: table => new
            {
                token = table.Column<string>(type: "char(48)", maxLength: 48, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                user_id = table.Column<long>(type: "bigint", nullable: false),
                created = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_password_reset_tokens", x => x.token);
                table.ForeignKey(
                    name: "fk_password_reset_tokens_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "recipes",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
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
                    .Annotation("MySql:CharSet", "utf8mb4"),
                created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                created_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                modified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                modified_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                revision = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipes", x => x.id);
                table.ForeignKey(
                    name: "fk_recipes_users_created_by_user_id",
                    column: x => x.created_by_user_id,
                    principalTable: "users",
                    principalColumn: "id");
                table.ForeignKey(
                    name: "fk_recipes_users_modified_by_user_id",
                    column: x => x.modified_by_user_id,
                    principalTable: "users",
                    principalColumn: "id");
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "ix_authentication_events_user_id",
            table: "authentication_events",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_password_reset_tokens_user_id",
            table: "password_reset_tokens",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_created_by_user_id",
            table: "recipes",
            column: "created_by_user_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_modified_by_user_id",
            table: "recipes",
            column: "modified_by_user_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("authentication_events");
        migrationBuilder.DropTable("password_reset_tokens");
        migrationBuilder.DropTable("recipes");
        migrationBuilder.DropTable("users");
    }
}
