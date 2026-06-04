using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    event_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    event_timestamp_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: false),
                    previous_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    employee_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.CheckConstraint("ck_users_email_or_employee_id", "email IS NOT NULL OR employee_id IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_users_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "account_lockouts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    failed_attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    locked_until_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_failed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_lockouts", x => x.id);
                    table.ForeignKey(
                        name: "FK_account_lockouts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "auth_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_token_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    issued_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    keep_me_logged_in = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auth_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_auth_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    used_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_used = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_password_reset_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_account_lockouts_locked_until_utc",
                table: "account_lockouts",
                column: "locked_until_utc");

            migrationBuilder.CreateIndex(
                name: "ux_account_lockouts_user_id",
                table: "account_lockouts",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_event_timestamp_utc",
                table: "audit_logs",
                column: "event_timestamp_utc");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_event_type",
                table: "audit_logs",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "ux_audit_logs_hash",
                table: "audit_logs",
                column: "hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_auth_sessions_expires_at_utc",
                table: "auth_sessions",
                column: "expires_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_auth_sessions_user_id",
                table: "auth_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ux_auth_sessions_session_token_id",
                table: "auth_sessions",
                column: "session_token_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_password_reset_tokens_expires_at_utc",
                table: "password_reset_tokens",
                column: "expires_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_password_reset_tokens_user_id",
                table: "password_reset_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ux_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_role_id",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ux_users_email",
                table: "users",
                column: "email",
                unique: true,
                filter: "email IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_users_employee_id",
                table: "users",
                column: "employee_id",
                unique: true,
                filter: "employee_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_lockouts");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "auth_sessions");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}