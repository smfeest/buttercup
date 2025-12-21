using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class BackfillChangeAndResetPasswordAuditEntries : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(
        @"INSERT INTO user_audit_entries (time, operation_type, target_id, actor_id, ip_address)
        SELECT time, 'change_password', user_id, user_id, ip_address
        FROM security_events
        WHERE event = 'password_change_success';

        INSERT INTO user_audit_entries (time, operation_type, target_id, actor_id, ip_address)
        SELECT time, 'reset_password', user_id, user_id, ip_address
        FROM security_events
        WHERE event = 'password_reset_success';");

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
