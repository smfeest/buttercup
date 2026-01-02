using Microsoft.EntityFrameworkCore.Migrations;

namespace Buttercup.EntityModel.Migrations;

public partial class BackfillUserAuditEntriesWithSecurityEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        CREATE TEMPORARY TABLE security_event_mappings (
            event VARCHAR(50) PRIMARY KEY,
            operation VARCHAR(30) NOT NULL,
            failure VARCHAR(30)
        );

        INSERT INTO security_event_mappings (event, operation, failure) VALUES
            ('access_token_issued', 'create_access_token', NULL),
            ('authentication_failure:incorrect_password', 'authenticate_password', 'incorrect_password'),
            ('authentication_failure:no_password_set', 'authenticate_password', 'no_password_set'),
            ('authentication_failure:user_deactivated', 'authenticate_password', 'user_deactivated'),
            ('authentication_success', 'authenticate_password', NULL),
            ('password_change_failure:incorrect_password', 'change_password', 'incorrect_password'),
            ('password_change_failure:no_password_set', 'change_password', 'no_password_set'),
            ('password_change_success', 'change_password', NULL),
            ('password_reset_failure:user_deactivated', 'reset_password', 'user_deactivated'),
            ('password_reset_link_sent', 'create_password_reset_token', NULL),
            ('password_reset_success', 'reset_password', NULL),
            ('sign_in', 'sign_in', NULL),
            ('sign_out', 'sign_out', NULL);

        INSERT INTO user_audit_entries (time, operation, target_id, actor_id, ip_address, failure)
        SELECT time, operation, user_id, user_id, ip_address, failure
        FROM security_events
        JOIN security_event_mappings ON security_events.event = security_event_mappings.event;

        DROP TEMPORARY TABLE security_event_mappings;
        """);
}
