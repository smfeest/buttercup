using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class BackfillUpdateCountOnUsers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("UPDATE users SET update_count = revision;");

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
