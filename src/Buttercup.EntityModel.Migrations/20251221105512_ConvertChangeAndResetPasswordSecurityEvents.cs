using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class ConvertChangeAndResetPasswordSecurityEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(
        @"INSERT INTO user_audit_entries (time, operation_type, target_id, actor_id, ip_address)
        SELECT time, 'change_password', user_id, user_id, ip_address
        FROM security_events
        WHERE event = 'password_change_success';

        INSERT INTO user_audit_entries (time, operation_type, target_id, actor_id, ip_address)
        SELECT time, 'reset_password', user_id, user_id, ip_address
        FROM security_events
        WHERE event = 'password_reset_success';

        DELETE FROM security_events
        WHERE event IN ('password_change_success', 'password_reset_success');");

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(
        @"INSERT INTO security_events (time, event, user_id, ip_address)
        SELECT time, 'password_change_success', actor_id, ip_address
        FROM user_audit_entries
        WHERE operation_type = 'change_password';

        INSERT INTO security_events (time, event, user_id, ip_address)
        SELECT time, 'password_reset_success', actor_id, ip_address
        FROM user_audit_entries
        WHERE operation_type = 'reset_password';

        DELETE FROM user_audit_entries
        WHERE operation_type IN ('change_password', 'reset_password');");
}
