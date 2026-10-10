using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Buttercup.EntityModel.Migrations;

public partial class BaselineSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:CollationDefinition:und-ci", "und-u-ks-level2,und-u-ks-level2,icu,False");

        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, collation: "und-ci"),
                email = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, collation: "und-ci"),
                hashed_password = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true, collation: "C"),
                password_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                security_stamp = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false, collation: "C"),
                time_zone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, collation: "C"),
                is_admin = table.Column<bool>(type: "boolean", nullable: false),
                created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                deactivated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                update_count = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "password_reset_tokens",
            columns: table => new
            {
                token = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false, collation: "C"),
                user_id = table.Column<long>(type: "bigint", nullable: false),
                created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
            });

        migrationBuilder.CreateTable(
            name: "recipes",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, collation: "und-ci"),
                preparation_minutes = table.Column<int>(type: "integer", nullable: true),
                cooking_minutes = table.Column<int>(type: "integer", nullable: true),
                servings = table.Column<int>(type: "integer", nullable: true),
                ingredients = table.Column<string>(type: "text", nullable: false, collation: "und-ci"),
                method = table.Column<string>(type: "text", nullable: false, collation: "und-ci"),
                suggestions = table.Column<string>(type: "text", nullable: true, collation: "und-ci"),
                remarks = table.Column<string>(type: "text", nullable: true, collation: "und-ci"),
                source = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true, collation: "und-ci"),
                created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                modified_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                deleted_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                update_count = table.Column<int>(type: "integer", nullable: false)
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
                    name: "fk_recipes_users_deleted_by_user_id",
                    column: x => x.deleted_by_user_id,
                    principalTable: "users",
                    principalColumn: "id");
                table.ForeignKey(
                    name: "fk_recipes_users_modified_by_user_id",
                    column: x => x.modified_by_user_id,
                    principalTable: "users",
                    principalColumn: "id");
            });

        migrationBuilder.CreateTable(
            name: "user_audit_entries",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                operation = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, collation: "C"),
                target_id = table.Column<long>(type: "bigint", nullable: false),
                actor_id = table.Column<long>(type: "bigint", nullable: false),
                ip_address = table.Column<IPAddress>(type: "inet", nullable: true),
                failure = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true, collation: "C")
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
            });

        migrationBuilder.CreateTable(
            name: "comments",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                recipe_id = table.Column<long>(type: "bigint", nullable: false),
                author_id = table.Column<long>(type: "bigint", nullable: true),
                body = table.Column<string>(type: "text", nullable: false, collation: "und-ci"),
                created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                deleted_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                update_count = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_comments", x => x.id);
                table.ForeignKey(
                    name: "fk_comments_recipes_recipe_id",
                    column: x => x.recipe_id,
                    principalTable: "recipes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_comments_users_author_id",
                    column: x => x.author_id,
                    principalTable: "users",
                    principalColumn: "id");
                table.ForeignKey(
                    name: "fk_comments_users_deleted_by_user_id",
                    column: x => x.deleted_by_user_id,
                    principalTable: "users",
                    principalColumn: "id");
            });

        migrationBuilder.CreateTable(
            name: "recipe_revisions",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                recipe_id = table.Column<long>(type: "bigint", nullable: false),
                title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, collation: "und-ci"),
                preparation_minutes = table.Column<int>(type: "integer", nullable: true),
                cooking_minutes = table.Column<int>(type: "integer", nullable: true),
                servings = table.Column<int>(type: "integer", nullable: true),
                ingredients = table.Column<string>(type: "text", nullable: false, collation: "und-ci"),
                method = table.Column<string>(type: "text", nullable: false, collation: "und-ci"),
                suggestions = table.Column<string>(type: "text", nullable: true, collation: "und-ci"),
                remarks = table.Column<string>(type: "text", nullable: true, collation: "und-ci"),
                source = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true, collation: "und-ci")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipe_revisions", x => x.id);
                table.ForeignKey(
                    name: "fk_recipe_revisions_recipes_recipe_id",
                    column: x => x.recipe_id,
                    principalTable: "recipes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "comment_revisions",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                comment_id = table.Column<long>(type: "bigint", nullable: false),
                body = table.Column<string>(type: "text", nullable: false, collation: "und-ci")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_comment_revisions", x => x.id);
                table.ForeignKey(
                    name: "fk_comment_revisions_comments_comment_id",
                    column: x => x.comment_id,
                    principalTable: "comments",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "recipe_audits",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                recipe_id = table.Column<long>(type: "bigint", nullable: false),
                time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                action = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, collation: "C"),
                revision_id = table.Column<long>(type: "bigint", nullable: true),
                actor_id = table.Column<long>(type: "bigint", nullable: true),
                ip_address = table.Column<IPAddress>(type: "inet", nullable: true)
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
            });

        migrationBuilder.CreateTable(
            name: "comment_audits",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                comment_id = table.Column<long>(type: "bigint", nullable: false),
                time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                action = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, collation: "C"),
                revision_id = table.Column<long>(type: "bigint", nullable: true),
                actor_id = table.Column<long>(type: "bigint", nullable: true),
                ip_address = table.Column<IPAddress>(type: "inet", nullable: true)
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
            });

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

        migrationBuilder.CreateIndex(
            name: "ix_comment_revisions_comment_id",
            table: "comment_revisions",
            column: "comment_id");

        migrationBuilder.CreateIndex(
            name: "ix_comments_author_id",
            table: "comments",
            column: "author_id");

        migrationBuilder.CreateIndex(
            name: "ix_comments_deleted",
            table: "comments",
            column: "deleted");

        migrationBuilder.CreateIndex(
            name: "ix_comments_deleted_by_user_id",
            table: "comments",
            column: "deleted_by_user_id");

        migrationBuilder.CreateIndex(
            name: "ix_comments_recipe_id",
            table: "comments",
            column: "recipe_id");

        migrationBuilder.CreateIndex(
            name: "ix_password_reset_tokens_user_id",
            table: "password_reset_tokens",
            column: "user_id");

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

        migrationBuilder.CreateIndex(
            name: "ix_recipe_revisions_recipe_id",
            table: "recipe_revisions",
            column: "recipe_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_created",
            table: "recipes",
            column: "created");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_created_by_user_id",
            table: "recipes",
            column: "created_by_user_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_deleted",
            table: "recipes",
            column: "deleted");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_deleted_by_user_id",
            table: "recipes",
            column: "deleted_by_user_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_modified",
            table: "recipes",
            column: "modified");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_modified_by_user_id",
            table: "recipes",
            column: "modified_by_user_id");

        migrationBuilder.CreateIndex(
            name: "ix_recipes_title",
            table: "recipes",
            column: "title");

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

        migrationBuilder.CreateIndex(
            name: "ix_users_created",
            table: "users",
            column: "created");

        migrationBuilder.CreateIndex(
            name: "ix_users_email",
            table: "users",
            column: "email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_users_modified",
            table: "users",
            column: "modified");

        migrationBuilder.CreateIndex(
            name: "ix_users_name",
            table: "users",
            column: "name");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("comment_audits");
        migrationBuilder.DropTable("password_reset_tokens");
        migrationBuilder.DropTable("recipe_audits");
        migrationBuilder.DropTable("user_audit_entries");
        migrationBuilder.DropTable("comment_revisions");
        migrationBuilder.DropTable("recipe_revisions");
        migrationBuilder.DropTable("comments");
        migrationBuilder.DropTable("recipes");
        migrationBuilder.DropTable("users");
    }
}
