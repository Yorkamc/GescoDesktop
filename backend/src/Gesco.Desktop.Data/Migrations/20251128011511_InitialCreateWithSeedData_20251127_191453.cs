using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Gesco.Desktop.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithSeedData_20251127_191453 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "activity_statuses",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inventory_movement_types",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    requires_justification = table.Column<bool>(type: "INTEGER", nullable: false),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_movement_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "login_attempts",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    attempted_email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    result = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ip_address = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    attempt_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    user_agent = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_login_attempts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "memberships",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    monthly_price = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    annual_price = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    user_limit = table.Column<int>(type: "INTEGER", nullable: false),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memberships", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notification_types",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    level = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organizations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    contact_email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    contact_phone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    address = table.Column<string>(type: "TEXT", nullable: true),
                    purchaser_name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    sync_status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    conflict_resolution = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    last_sync_error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_methods",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    requires_reference = table.Column<bool>(type: "INTEGER", nullable: false),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_methods", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sales_statuses",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subscription_statuses",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    allows_system_usage = table.Column<bool>(type: "INTEGER", nullable: false),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "service_categories",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    sync_status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    conflict_resolution = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    last_sync_error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_service_categories_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "system_configuration",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    value = table.Column<string>(type: "TEXT", nullable: false),
                    data_type = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    is_editable = table.Column<bool>(type: "INTEGER", nullable: false),
                    access_level = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    display_order = table.Column<int>(type: "INTEGER", nullable: false),
                    validation_pattern = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    min_value = table.Column<decimal>(type: "TEXT", nullable: true),
                    max_value = table.Column<decimal>(type: "TEXT", nullable: true),
                    allowed_values = table.Column<string>(type: "TEXT", nullable: true),
                    is_sensitive = table.Column<bool>(type: "INTEGER", nullable: false),
                    environment = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    restart_required = table.Column<bool>(type: "INTEGER", nullable: false),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_configuration", x => x.id);
                    table.ForeignKey(
                        name: "FK_system_configuration_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    username = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    email_verified_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    password = table.Column<string>(type: "TEXT", nullable: false),
                    full_name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    phone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    first_login = table.Column<bool>(type: "INTEGER", nullable: false),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    role_id = table.Column<long>(type: "INTEGER", nullable: false),
                    first_login_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    last_login_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    sync_status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    conflict_resolution = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    last_sync_error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_users_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    membership_id = table.Column<long>(type: "INTEGER", nullable: false),
                    subscription_status_id = table.Column<long>(type: "INTEGER", nullable: false),
                    start_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    expiration_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    grace_period_end = table.Column<DateTime>(type: "TEXT", nullable: false),
                    cancellation_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    sync_status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    conflict_resolution = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    last_sync_error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_subscriptions_memberships_membership_id",
                        column: x => x.membership_id,
                        principalTable: "memberships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscriptions_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscriptions_subscription_statuses_subscription_status_id",
                        column: x => x.subscription_status_id,
                        principalTable: "subscription_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "activities",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    start_date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    end_date = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    end_time = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    location = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    activity_status_id = table.Column<long>(type: "INTEGER", nullable: false),
                    manager_user_id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    sync_status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    conflict_resolution = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    last_sync_error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activities", x => x.id);
                    table.ForeignKey(
                        name: "FK_activities_activity_statuses_activity_status_id",
                        column: x => x.activity_status_id,
                        principalTable: "activity_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_activities_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_activities_users_manager_user_id",
                        column: x => x.manager_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "api_activity_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    method = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    endpoint = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ip_address = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "TEXT", nullable: true),
                    request_data = table.Column<string>(type: "TEXT", nullable: true),
                    response_status = table.Column<int>(type: "INTEGER", nullable: false),
                    response_time_ms = table.Column<decimal>(type: "TEXT", nullable: true),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    module = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_activity_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_api_activity_logs_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_api_activity_logs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "desktop_clients",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    client_name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    app_version = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    user_id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    last_sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_connection = table.Column<DateTime>(type: "TEXT", nullable: true),
                    status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ip_address = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    sync_interval_seconds = table.Column<int>(type: "INTEGER", nullable: false),
                    read_only = table.Column<bool>(type: "INTEGER", nullable: false),
                    receive_notifications = table.Column<bool>(type: "INTEGER", nullable: false),
                    registered_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_desktop_clients", x => x.id);
                    table.ForeignKey(
                        name: "FK_desktop_clients_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_desktop_clients_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    notification_type_id = table.Column<long>(type: "INTEGER", nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "TEXT", nullable: false),
                    additional_data = table.Column<string>(type: "TEXT", nullable: true),
                    is_read = table.Column<bool>(type: "INTEGER", nullable: false),
                    read_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    important = table.Column<bool>(type: "INTEGER", nullable: false),
                    scheduled_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    expiration_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    delivery_channels = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_notification_types_notification_type_id",
                        column: x => x.notification_type_id,
                        principalTable: "notification_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "oauth_access_tokens",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    user_id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    client_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    scopes = table.Column<string>(type: "TEXT", nullable: true),
                    revoked = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    expires_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_oauth_access_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_oauth_access_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "user_sessions",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    user_id = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    access_token = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    refresh_token = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    token_uuid = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    ip_address = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "TEXT", nullable: true),
                    start_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    last_access_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    expiration_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    end_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    end_reason = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "activation_keys",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    activation_code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    subscription_id = table.Column<long>(type: "INTEGER", nullable: false),
                    is_generated = table.Column<bool>(type: "INTEGER", nullable: false),
                    is_used = table.Column<bool>(type: "INTEGER", nullable: false),
                    is_expired = table.Column<bool>(type: "INTEGER", nullable: false),
                    is_revoked = table.Column<bool>(type: "INTEGER", nullable: false),
                    generated_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    expiration_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    used_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    revoked_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    max_uses = table.Column<int>(type: "INTEGER", nullable: false),
                    current_uses = table.Column<int>(type: "INTEGER", nullable: false),
                    generation_batch = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "TEXT", nullable: true),
                    used_by_organization_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    used_by_user_id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    activation_ip = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    generated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    revoked_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    revocation_reason = table.Column<string>(type: "TEXT", nullable: true),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activation_keys", x => x.id);
                    table.ForeignKey(
                        name: "FK_activation_keys_subscriptions_subscription_id",
                        column: x => x.subscription_id,
                        principalTable: "subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_activation_keys_users_generated_by",
                        column: x => x.generated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_activation_keys_users_revoked_by",
                        column: x => x.revoked_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_activation_keys_users_used_by_user_id",
                        column: x => x.used_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "activity_categories",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    activity_id = table.Column<long>(type: "INTEGER", nullable: false),
                    service_category_id = table.Column<long>(type: "INTEGER", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    sync_status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    conflict_resolution = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    last_sync_error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_activity_categories_activities_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_activity_categories_service_categories_service_category_id",
                        column: x => x.service_category_id,
                        principalTable: "service_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "activity_closures",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    activity_id = table.Column<long>(type: "INTEGER", nullable: false),
                    closure_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    duration_hours = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    total_registers = table.Column<int>(type: "INTEGER", nullable: false),
                    registers_with_differences = table.Column<int>(type: "INTEGER", nullable: false),
                    total_sales = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    total_transactions = table.Column<int>(type: "INTEGER", nullable: false),
                    total_items_sold = table.Column<int>(type: "INTEGER", nullable: false),
                    out_of_stock_items = table.Column<int>(type: "INTEGER", nullable: false),
                    items_with_stock = table.Column<int>(type: "INTEGER", nullable: false),
                    total_remaining_units = table.Column<int>(type: "INTEGER", nullable: false),
                    final_inventory_value = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    shrinkage_value = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    closed_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    supervised_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    observations = table.Column<string>(type: "TEXT", nullable: true),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    sync_status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    conflict_resolution = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    last_sync_error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_closures", x => x.id);
                    table.ForeignKey(
                        name: "FK_activity_closures_activities_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_activity_closures_users_closed_by",
                        column: x => x.closed_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_activity_closures_users_supervised_by",
                        column: x => x.supervised_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "cash_registers",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    activity_id = table.Column<long>(type: "INTEGER", nullable: false),
                    register_number = table.Column<int>(type: "INTEGER", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    is_open = table.Column<bool>(type: "INTEGER", nullable: false),
                    opened_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    closed_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    operator_user_id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    supervisor_user_id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    sync_status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    conflict_resolution = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    last_sync_error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cash_registers", x => x.id);
                    table.ForeignKey(
                        name: "FK_cash_registers_activities_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cash_registers_users_operator_user_id",
                        column: x => x.operator_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_cash_registers_users_supervisor_user_id",
                        column: x => x.supervisor_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "sales_combos",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    activity_id = table.Column<long>(type: "INTEGER", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    combo_price = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    sync_status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    conflict_resolution = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    last_sync_error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_combos", x => x.id);
                    table.ForeignKey(
                        name: "FK_sales_combos_activities_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sync_queue",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    client_id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    affected_table = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    record_id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    operation = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    change_data = table.Column<string>(type: "TEXT", nullable: false),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    attempts = table.Column<int>(type: "INTEGER", nullable: false),
                    max_attempts = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    sent_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    confirmed_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    expires_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    priority = table.Column<int>(type: "INTEGER", nullable: false),
                    batch_id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    error_message = table.Column<string>(type: "TEXT", nullable: true),
                    error_code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_queue", x => x.id);
                    table.ForeignKey(
                        name: "FK_sync_queue_desktop_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "desktop_clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sync_queue_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sync_versions",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    table_name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    record_id = table.Column<string>(type: "TEXT", nullable: false),
                    version = table.Column<long>(type: "INTEGER", nullable: false),
                    operation = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    change_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    change_data = table.Column<string>(type: "TEXT", nullable: true),
                    changed_by_user = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    origin_client_id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_versions", x => x.id);
                    table.ForeignKey(
                        name: "FK_sync_versions_desktop_clients_origin_client_id",
                        column: x => x.origin_client_id,
                        principalTable: "desktop_clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_sync_versions_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sync_versions_users_changed_by_user",
                        column: x => x.changed_by_user,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "oauth_refresh_tokens",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    access_token_id = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    revoked = table.Column<bool>(type: "INTEGER", nullable: false),
                    expires_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_oauth_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_oauth_refresh_tokens_oauth_access_tokens_access_token_id",
                        column: x => x.access_token_id,
                        principalTable: "oauth_access_tokens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "activation_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    activation_key_id = table.Column<long>(type: "INTEGER", nullable: false),
                    activation_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    subscription_start_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    subscription_end_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    activated_by_user_id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    activation_ip = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "TEXT", nullable: true),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    deactivation_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    deactivation_reason = table.Column<string>(type: "TEXT", nullable: true),
                    deactivated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activation_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_activation_history_activation_keys_activation_key_id",
                        column: x => x.activation_key_id,
                        principalTable: "activation_keys",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_activation_history_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_activation_history_users_activated_by_user_id",
                        column: x => x.activated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_activation_history_users_deactivated_by",
                        column: x => x.deactivated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "category_products",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    activity_category_id = table.Column<long>(type: "INTEGER", nullable: true),
                    code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    unit_price = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    initial_quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    current_quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    alert_quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    sync_status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    conflict_resolution = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    last_sync_error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_products", x => x.id);
                    table.ForeignKey(
                        name: "FK_category_products_activity_categories_activity_category_id",
                        column: x => x.activity_category_id,
                        principalTable: "activity_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cash_register_closures",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    cash_register_id = table.Column<long>(type: "INTEGER", nullable: false),
                    opening_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    closing_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    total_transactions = table.Column<int>(type: "INTEGER", nullable: false),
                    total_items_sold = table.Column<int>(type: "INTEGER", nullable: false),
                    total_sales_amount = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    cash_calculated = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    cards_calculated = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    sinpe_calculated = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    cash_declared = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    cash_difference = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    closed_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    supervised_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    observations = table.Column<string>(type: "TEXT", nullable: true),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    sync_status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    conflict_resolution = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    last_sync_error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cash_register_closures", x => x.id);
                    table.ForeignKey(
                        name: "FK_cash_register_closures_cash_registers_cash_register_id",
                        column: x => x.cash_register_id,
                        principalTable: "cash_registers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cash_register_closures_users_closed_by",
                        column: x => x.closed_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_cash_register_closures_users_supervised_by",
                        column: x => x.supervised_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "sales_transactions",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    cash_register_id = table.Column<long>(type: "INTEGER", nullable: false),
                    transaction_number = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    invoice_number = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    sales_status_id = table.Column<long>(type: "INTEGER", nullable: false),
                    transaction_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    total_amount = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    sync_status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    conflict_resolution = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    last_sync_error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_sales_transactions_cash_registers_cash_register_id",
                        column: x => x.cash_register_id,
                        principalTable: "cash_registers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sales_transactions_sales_statuses_sales_status_id",
                        column: x => x.sales_status_id,
                        principalTable: "sales_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "combo_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    combo_id = table.Column<long>(type: "INTEGER", nullable: false),
                    product_id = table.Column<long>(type: "INTEGER", nullable: false),
                    quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    sync_status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    conflict_resolution = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    last_sync_error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_combo_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_combo_items_category_products_product_id",
                        column: x => x.product_id,
                        principalTable: "category_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_combo_items_sales_combos_combo_id",
                        column: x => x.combo_id,
                        principalTable: "sales_combos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inventory_movements",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    product_id = table.Column<long>(type: "INTEGER", nullable: false),
                    movement_type_id = table.Column<long>(type: "INTEGER", nullable: false),
                    quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    previous_quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    new_quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    unit_cost = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    total_value = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    sales_transaction_id = table.Column<long>(type: "INTEGER", nullable: true),
                    justification = table.Column<string>(type: "TEXT", nullable: true),
                    performed_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    authorized_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    movement_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    sync_status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    conflict_resolution = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    last_sync_error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_movements", x => x.id);
                    table.ForeignKey(
                        name: "FK_inventory_movements_category_products_product_id",
                        column: x => x.product_id,
                        principalTable: "category_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inventory_movements_inventory_movement_types_movement_type_id",
                        column: x => x.movement_type_id,
                        principalTable: "inventory_movement_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inventory_movements_sales_transactions_sales_transaction_id",
                        column: x => x.sales_transaction_id,
                        principalTable: "sales_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inventory_movements_users_authorized_by",
                        column: x => x.authorized_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_inventory_movements_users_performed_by",
                        column: x => x.performed_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "transaction_details",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    sales_transaction_id = table.Column<long>(type: "INTEGER", nullable: false),
                    product_id = table.Column<long>(type: "INTEGER", nullable: true),
                    combo_id = table.Column<long>(type: "INTEGER", nullable: true),
                    quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    unit_price = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    total_amount = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    is_combo = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    sync_status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    conflict_resolution = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    last_sync_error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_details", x => x.id);
                    table.ForeignKey(
                        name: "FK_transaction_details_category_products_product_id",
                        column: x => x.product_id,
                        principalTable: "category_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_details_sales_combos_combo_id",
                        column: x => x.combo_id,
                        principalTable: "sales_combos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_details_sales_transactions_sales_transaction_id",
                        column: x => x.sales_transaction_id,
                        principalTable: "sales_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transaction_payments",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    sales_transaction_id = table.Column<long>(type: "INTEGER", nullable: false),
                    payment_method_id = table.Column<long>(type: "INTEGER", nullable: false),
                    amount = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    reference = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    processed_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    processed_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    created_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    sync_status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    conflict_resolution = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    last_sync_error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_payments", x => x.id);
                    table.ForeignKey(
                        name: "FK_transaction_payments_payment_methods_payment_method_id",
                        column: x => x.payment_method_id,
                        principalTable: "payment_methods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_payments_sales_transactions_sales_transaction_id",
                        column: x => x.sales_transaction_id,
                        principalTable: "sales_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_payments_users_processed_by",
                        column: x => x.processed_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "activity_statuses",
                columns: new[] { "id", "active", "created_at", "created_by", "description", "name", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { 1L, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3069), null, "Actividad no iniciada", "No Iniciada", null, null },
                    { 2L, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3071), null, "Actividad en desarrollo", "En Progreso", null, null },
                    { 3L, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3074), null, "Actividad finalizada", "Completada", null, null },
                    { 4L, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3077), null, "Actividad cancelada", "Cancelada", null, null }
                });

            migrationBuilder.InsertData(
                table: "inventory_movement_types",
                columns: new[] { "id", "active", "created_at", "created_by", "description", "name", "requires_justification", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { 1L, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3212), null, "Ingreso de mercadería al inventario", "Entrada", false, null, null },
                    { 2L, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3215), null, "Salida por venta de producto", "Venta", false, null, null },
                    { 3L, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3217), null, "Ajuste de inventario por diferencias", "Ajuste", true, null, null },
                    { 4L, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3219), null, "Devolución de producto", "Devolución", true, null, null },
                    { 5L, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3221), null, "Producto dañado o vencido", "Merma", true, null, null }
                });

            migrationBuilder.InsertData(
                table: "memberships",
                columns: new[] { "id", "active", "annual_price", "created_at", "created_by", "description", "monthly_price", "name", "updated_at", "updated_by", "user_limit" },
                values: new object[,]
                {
                    { 1L, true, 299.99m, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3316), null, "Plan básico con funciones esenciales", 29.99m, "Básica", null, null, 5 },
                    { 2L, true, 599.99m, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3320), null, "Plan profesional con funciones avanzadas", 59.99m, "Profesional", null, null, 25 }
                });

            migrationBuilder.InsertData(
                table: "notification_types",
                columns: new[] { "id", "active", "code", "created_at", "created_by", "description", "level", "name", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { 1L, true, "low_stock", new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3414), null, "El inventario del producto está bajo", "warning", "Alerta de Stock Bajo", null, null },
                    { 2L, true, "activity_reminder", new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3417), null, "Notificación de actividad próxima", "info", "Recordatorio de Actividad", null, null },
                    { 3L, true, "system_alert", new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3419), null, "Notificación crítica del sistema", "critical", "Alerta del Sistema", null, null },
                    { 4L, true, "sync_error", new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3422), null, "Fallo en la sincronización de datos", "error", "Error de Sincronización", null, null }
                });

            migrationBuilder.InsertData(
                table: "organizations",
                columns: new[] { "id", "active", "address", "conflict_resolution", "contact_email", "contact_phone", "created_at", "created_by", "integrity_hash", "last_sync", "last_sync_error", "name", "purchaser_name", "sync_status", "sync_version", "updated_at", "updated_by" },
                values: new object[] { new Guid("8a36002b-299b-43ad-8b01-b9d53bc1165e"), true, "San José, Costa Rica", null, "demo@gesco.com", "2222-2222", new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(2726), null, null, null, null, "GESCO Demo", "Administrador Demo", "pending", 1L, null, null });

            migrationBuilder.InsertData(
                table: "payment_methods",
                columns: new[] { "id", "active", "created_at", "created_by", "name", "requires_reference", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { 1L, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3159), null, "Efectivo", false, null, null },
                    { 2L, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3162), null, "Tarjeta", true, null, null },
                    { 3L, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3172), null, "SINPE Móvil", true, null, null },
                    { 4L, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3174), null, "Transferencia", true, null, null }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "active", "created_at", "created_by", "description", "name", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { 1L, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(2959), null, "Acceso completo al sistema", "Administrador", null, null },
                    { 2L, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(2963), null, "Acceso a ventas y caja", "Vendedor", null, null },
                    { 3L, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(2965), null, "Supervisión de actividades", "Supervisor", null, null }
                });

            migrationBuilder.InsertData(
                table: "sales_statuses",
                columns: new[] { "id", "active", "created_at", "created_by", "description", "name", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { 1L, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3118), null, "Venta pendiente de procesar", "Pending", null, null },
                    { 2L, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3120), null, "Venta completada exitosamente", "Completed", null, null },
                    { 3L, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3123), null, "Venta anulada", "Cancelled", null, null }
                });

            migrationBuilder.InsertData(
                table: "subscription_statuses",
                columns: new[] { "id", "active", "allows_system_usage", "created_at", "created_by", "description", "name", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { 1L, true, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3272), null, "Suscripción activa", "Activa", null, null },
                    { 2L, true, false, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3275), null, "Suscripción suspendida", "Suspendida", null, null },
                    { 3L, true, false, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3277), null, "Suscripción cancelada", "Cancelada", null, null },
                    { 4L, true, true, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3280), null, "Período de prueba", "Prueba", null, null }
                });

            migrationBuilder.InsertData(
                table: "system_configuration",
                columns: new[] { "id", "access_level", "allowed_values", "category", "created_at", "created_by", "data_type", "description", "display_order", "environment", "is_editable", "is_sensitive", "key", "max_value", "min_value", "organization_id", "restart_required", "updated_at", "updated_by", "validation_pattern", "value" },
                values: new object[,]
                {
                    { 1L, "admin", null, "system", new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3458), null, "string", "Versión del sistema", 0, "all", false, false, "system.version", null, null, null, false, null, null, null, "1.0.0" },
                    { 2L, "admin", null, "backup", new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3461), null, "int", "Intervalo de respaldo en horas", 0, "all", true, false, "backup.interval_hours", null, null, null, false, null, null, null, "6" },
                    { 3L, "admin", null, "sales", new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3464), null, "bool", "Permitir ventas con stock negativo", 0, "all", true, false, "sales.allow_negative_stock", null, null, null, false, null, null, null, "false" },
                    { 4L, "admin", null, "sales", new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3467), null, "decimal", "Tasa de impuesto por defecto (IVA)", 0, "all", true, false, "sales.default_tax_rate", null, null, null, false, null, null, null, "13" }
                });

            migrationBuilder.InsertData(
                table: "service_categories",
                columns: new[] { "id", "active", "conflict_resolution", "created_at", "created_by", "description", "integrity_hash", "last_sync", "last_sync_error", "name", "organization_id", "sync_status", "sync_version", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { 1L, true, null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3518), null, "Comidas y snacks", null, null, null, "Alimentos", new Guid("8a36002b-299b-43ad-8b01-b9d53bc1165e"), "pending", 1L, null, null },
                    { 2L, true, null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3521), null, "Bebidas alcohólicas y no alcohólicas", null, null, null, "Bebidas", new Guid("8a36002b-299b-43ad-8b01-b9d53bc1165e"), "pending", 1L, null, null },
                    { 3L, true, null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3524), null, "Recuerdos y merchandising", null, null, null, "Souvenirs", new Guid("8a36002b-299b-43ad-8b01-b9d53bc1165e"), "pending", 1L, null, null }
                });

            migrationBuilder.InsertData(
                table: "subscriptions",
                columns: new[] { "id", "cancellation_date", "conflict_resolution", "created_at", "created_by", "expiration_date", "grace_period_end", "integrity_hash", "last_sync", "last_sync_error", "membership_id", "organization_id", "start_date", "subscription_status_id", "sync_status", "sync_version", "updated_at", "updated_by" },
                values: new object[] { 1L, null, null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3375), null, new DateTime(2026, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3362), new DateTime(2026, 12, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3369), null, null, null, 2L, new Guid("8a36002b-299b-43ad-8b01-b9d53bc1165e"), new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3361), 1L, "pending", 1L, null, null });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "active", "conflict_resolution", "created_at", "created_by", "email", "email_verified_at", "first_login", "first_login_at", "full_name", "integrity_hash", "last_login_at", "last_sync", "last_sync_error", "organization_id", "password", "phone", "role_id", "sync_status", "sync_version", "updated_at", "updated_by", "username" },
                values: new object[] { "118640123", true, null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3031), null, "admin@gesco.com", null, true, null, "Administrador del Sistema", null, null, null, null, new Guid("8a36002b-299b-43ad-8b01-b9d53bc1165e"), "$2a$12$LQV.K4/OOOgwdEXCfC7jC.QLwpZ9HkqhXfOr9p6mTyYFEYGHZcP/a", "8888-8888", 1L, "pending", 1L, null, null, "admin" });

            migrationBuilder.InsertData(
                table: "activities",
                columns: new[] { "id", "activity_status_id", "conflict_resolution", "created_at", "created_by", "description", "end_date", "end_time", "integrity_hash", "last_sync", "last_sync_error", "location", "manager_user_id", "name", "organization_id", "start_date", "start_time", "sync_status", "sync_version", "updated_at", "updated_by" },
                values: new object[] { 1L, 1L, null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3794), "118640123", "Festival anual de música en vivo", new DateOnly(2025, 12, 29), new TimeOnly(23, 0, 0), null, null, null, "Parque Central, San José", "118640123", "Festival de Música 2025", new Guid("8a36002b-299b-43ad-8b01-b9d53bc1165e"), new DateOnly(2025, 12, 27), new TimeOnly(14, 0, 0), "pending", 1L, null, null });

            migrationBuilder.InsertData(
                table: "activity_categories",
                columns: new[] { "id", "activity_id", "conflict_resolution", "created_at", "created_by", "integrity_hash", "last_sync", "last_sync_error", "service_category_id", "sync_status", "sync_version", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { 1L, 1L, null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3844), null, null, null, null, 1L, "pending", 1L, null, null },
                    { 2L, 1L, null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3846), null, null, null, null, 2L, "pending", 1L, null, null },
                    { 3L, 1L, null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3849), null, null, null, null, 3L, "pending", 1L, null, null }
                });

            migrationBuilder.InsertData(
                table: "cash_registers",
                columns: new[] { "id", "activity_id", "closed_at", "conflict_resolution", "created_at", "created_by", "integrity_hash", "is_open", "last_sync", "last_sync_error", "location", "name", "opened_at", "operator_user_id", "register_number", "supervisor_user_id", "sync_status", "sync_version", "updated_at", "updated_by" },
                values: new object[] { 1L, 1L, null, null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3997), null, null, false, null, null, "Zona de ventas", "Caja Principal", null, null, 1, null, "pending", 1L, null, null });

            migrationBuilder.InsertData(
                table: "category_products",
                columns: new[] { "id", "active", "activity_category_id", "alert_quantity", "code", "conflict_resolution", "created_at", "created_by", "current_quantity", "description", "initial_quantity", "integrity_hash", "last_sync", "last_sync_error", "name", "sync_status", "sync_version", "unit_price", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { 1L, true, 1L, 20, "ALM-001", null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3909), null, 100, "Hamburguesa con queso, lechuga y tomate", 100, null, null, null, "Hamburguesa Clásica", "pending", 1L, 3500m, null, null },
                    { 2L, true, 1L, 30, "ALM-002", null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3913), null, 150, "Perro caliente con salsa especial", 150, null, null, null, "Hot Dog", "pending", 1L, 2000m, null, null },
                    { 3L, true, 1L, 40, "ALM-003", null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3917), null, 200, "Porción de papas fritas", 200, null, null, null, "Papas Fritas", "pending", 1L, 1500m, null, null },
                    { 4L, true, 1L, 15, "ALM-004", null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3921), null, 100, "Pizza individual", 100, null, null, null, "Pizza Personal", "pending", 1L, 4000m, null, null },
                    { 5L, true, 2L, 40, "BEB-001", null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3925), null, 200, "Cerveza nacional 355ml", 200, null, null, null, "Cerveza Nacional", "pending", 1L, 2500m, null, null },
                    { 6L, true, 2L, 30, "BEB-002", null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3929), null, 150, "Refresco natural de frutas", 150, null, null, null, "Refresco Natural", "pending", 1L, 1500m, null, null },
                    { 7L, true, 2L, 60, "BEB-003", null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3933), null, 300, "Agua purificada 500ml", 300, null, null, null, "Agua Embotellada", "pending", 1L, 1000m, null, null },
                    { 8L, true, 2L, 20, "BEB-004", null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3937), null, 100, "Café costarricense premium", 100, null, null, null, "Café Premium", "pending", 1L, 2000m, null, null },
                    { 9L, true, 3L, 10, "SOU-001", null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3940), null, 50, "Camiseta oficial del evento", 50, null, null, null, "Camiseta Festival 2025", "pending", 1L, 8000m, null, null },
                    { 10L, true, 3L, 10, "SOU-002", null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3945), null, 40, "Gorra bordada del festival", 40, null, null, null, "Gorra Edición Especial", "pending", 1L, 6000m, null, null },
                    { 11L, true, 3L, 5, "SOU-003", null, new DateTime(2025, 11, 28, 1, 15, 10, 835, DateTimeKind.Utc).AddTicks(3949), null, 30, "Taza cerámica del evento", 30, null, null, null, "Taza Conmemorativa", "pending", 1L, 5000m, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_activation_history_activated_by_user_id",
                table: "activation_history",
                column: "activated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_activation_history_activation_key_id",
                table: "activation_history",
                column: "activation_key_id");

            migrationBuilder.CreateIndex(
                name: "IX_activation_history_deactivated_by",
                table: "activation_history",
                column: "deactivated_by");

            migrationBuilder.CreateIndex(
                name: "IX_activation_history_organization_id",
                table: "activation_history",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_activation_keys_generated_by",
                table: "activation_keys",
                column: "generated_by");

            migrationBuilder.CreateIndex(
                name: "IX_activation_keys_revoked_by",
                table: "activation_keys",
                column: "revoked_by");

            migrationBuilder.CreateIndex(
                name: "IX_activation_keys_subscription_id",
                table: "activation_keys",
                column: "subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_activation_keys_used_by_user_id",
                table: "activation_keys",
                column: "used_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_activities_sync_tracking",
                table: "activities",
                columns: new[] { "sync_version", "last_sync" });

            migrationBuilder.CreateIndex(
                name: "IX_activities_activity_status_id",
                table: "activities",
                column: "activity_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_activities_manager_user_id",
                table: "activities",
                column: "manager_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_activities_organization_id",
                table: "activities",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_activity_categories_activity_id",
                table: "activity_categories",
                column: "activity_id");

            migrationBuilder.CreateIndex(
                name: "IX_activity_categories_service_category_id",
                table: "activity_categories",
                column: "service_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_activity_closures_activity_id",
                table: "activity_closures",
                column: "activity_id");

            migrationBuilder.CreateIndex(
                name: "IX_activity_closures_closed_by",
                table: "activity_closures",
                column: "closed_by");

            migrationBuilder.CreateIndex(
                name: "IX_activity_closures_supervised_by",
                table: "activity_closures",
                column: "supervised_by");

            migrationBuilder.CreateIndex(
                name: "idx_api_logs_org_date",
                table: "api_activity_logs",
                columns: new[] { "organization_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_api_activity_logs_user_id",
                table: "api_activity_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_cash_register_closures_cash_register_id",
                table: "cash_register_closures",
                column: "cash_register_id");

            migrationBuilder.CreateIndex(
                name: "IX_cash_register_closures_closed_by",
                table: "cash_register_closures",
                column: "closed_by");

            migrationBuilder.CreateIndex(
                name: "IX_cash_register_closures_supervised_by",
                table: "cash_register_closures",
                column: "supervised_by");

            migrationBuilder.CreateIndex(
                name: "IX_cash_registers_activity_id",
                table: "cash_registers",
                column: "activity_id");

            migrationBuilder.CreateIndex(
                name: "IX_cash_registers_operator_user_id",
                table: "cash_registers",
                column: "operator_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_cash_registers_supervisor_user_id",
                table: "cash_registers",
                column: "supervisor_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_category_products_activity_category_id",
                table: "category_products",
                column: "activity_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_combo_items_combo_id",
                table: "combo_items",
                column: "combo_id");

            migrationBuilder.CreateIndex(
                name: "IX_combo_items_product_id",
                table: "combo_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_desktop_clients_id",
                table: "desktop_clients",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_desktop_clients_organization_id",
                table: "desktop_clients",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_desktop_clients_user_id",
                table: "desktop_clients",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_movements_authorized_by",
                table: "inventory_movements",
                column: "authorized_by");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_movements_movement_type_id",
                table: "inventory_movements",
                column: "movement_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_movements_performed_by",
                table: "inventory_movements",
                column: "performed_by");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_movements_product_id",
                table: "inventory_movements",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_movements_sales_transaction_id",
                table: "inventory_movements",
                column: "sales_transaction_id");

            migrationBuilder.CreateIndex(
                name: "idx_login_attempts_date",
                table: "login_attempts",
                column: "attempt_date");

            migrationBuilder.CreateIndex(
                name: "idx_login_attempts_email_date",
                table: "login_attempts",
                columns: new[] { "attempted_email", "attempt_date" });

            migrationBuilder.CreateIndex(
                name: "idx_login_attempts_ip_date",
                table: "login_attempts",
                columns: new[] { "ip_address", "attempt_date" });

            migrationBuilder.CreateIndex(
                name: "idx_notifications_org_status",
                table: "notifications",
                columns: new[] { "organization_id", "is_read", "important" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_notification_type_id",
                table: "notifications",
                column: "notification_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_oauth_access_tokens_user_id",
                table: "oauth_access_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_oauth_refresh_tokens_access_token_id",
                table: "oauth_refresh_tokens",
                column: "access_token_id");

            migrationBuilder.CreateIndex(
                name: "idx_organizations_sync_tracking",
                table: "organizations",
                columns: new[] { "sync_version", "last_sync" });

            migrationBuilder.CreateIndex(
                name: "IX_sales_combos_activity_id",
                table: "sales_combos",
                column: "activity_id");

            migrationBuilder.CreateIndex(
                name: "IX_sales_transactions_cash_register_id",
                table: "sales_transactions",
                column: "cash_register_id");

            migrationBuilder.CreateIndex(
                name: "IX_sales_transactions_sales_status_id",
                table: "sales_transactions",
                column: "sales_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_categories_organization_id",
                table: "service_categories",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_membership_id",
                table: "subscriptions",
                column: "membership_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_organization_id",
                table: "subscriptions",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_subscription_status_id",
                table: "subscriptions",
                column: "subscription_status_id");

            migrationBuilder.CreateIndex(
                name: "idx_sync_queue_unique",
                table: "sync_queue",
                columns: new[] { "client_id", "affected_table", "record_id", "sync_version" });

            migrationBuilder.CreateIndex(
                name: "IX_sync_queue_organization_id",
                table: "sync_queue",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_sync_versions_changed_by_user",
                table: "sync_versions",
                column: "changed_by_user");

            migrationBuilder.CreateIndex(
                name: "IX_sync_versions_organization_id",
                table: "sync_versions",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_sync_versions_origin_client_id",
                table: "sync_versions",
                column: "origin_client_id");

            migrationBuilder.CreateIndex(
                name: "IX_system_configuration_organization_id",
                table: "system_configuration",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_details_combo_id",
                table: "transaction_details",
                column: "combo_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_details_product_id",
                table: "transaction_details",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_details_sales_transaction_id",
                table: "transaction_details",
                column: "sales_transaction_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_payments_payment_method_id",
                table: "transaction_payments",
                column: "payment_method_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_payments_processed_by",
                table: "transaction_payments",
                column: "processed_by");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_payments_sales_transaction_id",
                table: "transaction_payments",
                column: "sales_transaction_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_sessions_access_token",
                table: "user_sessions",
                column: "access_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_user_sessions_refresh_token",
                table: "user_sessions",
                column: "refresh_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_user_sessions_user_active",
                table: "user_sessions",
                columns: new[] { "user_id", "active", "last_access_date" });

            migrationBuilder.CreateIndex(
                name: "idx_users_cedula_unique",
                table: "users",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_users_email_unique",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_users_org_username_unique",
                table: "users",
                columns: new[] { "organization_id", "username" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_users_sync_tracking",
                table: "users",
                columns: new[] { "sync_version", "last_sync" });

            migrationBuilder.CreateIndex(
                name: "IX_users_role_id",
                table: "users",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activation_history");

            migrationBuilder.DropTable(
                name: "activity_closures");

            migrationBuilder.DropTable(
                name: "api_activity_logs");

            migrationBuilder.DropTable(
                name: "cash_register_closures");

            migrationBuilder.DropTable(
                name: "combo_items");

            migrationBuilder.DropTable(
                name: "inventory_movements");

            migrationBuilder.DropTable(
                name: "login_attempts");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "oauth_refresh_tokens");

            migrationBuilder.DropTable(
                name: "sync_queue");

            migrationBuilder.DropTable(
                name: "sync_versions");

            migrationBuilder.DropTable(
                name: "system_configuration");

            migrationBuilder.DropTable(
                name: "transaction_details");

            migrationBuilder.DropTable(
                name: "transaction_payments");

            migrationBuilder.DropTable(
                name: "user_sessions");

            migrationBuilder.DropTable(
                name: "activation_keys");

            migrationBuilder.DropTable(
                name: "inventory_movement_types");

            migrationBuilder.DropTable(
                name: "notification_types");

            migrationBuilder.DropTable(
                name: "oauth_access_tokens");

            migrationBuilder.DropTable(
                name: "desktop_clients");

            migrationBuilder.DropTable(
                name: "category_products");

            migrationBuilder.DropTable(
                name: "sales_combos");

            migrationBuilder.DropTable(
                name: "payment_methods");

            migrationBuilder.DropTable(
                name: "sales_transactions");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "activity_categories");

            migrationBuilder.DropTable(
                name: "cash_registers");

            migrationBuilder.DropTable(
                name: "sales_statuses");

            migrationBuilder.DropTable(
                name: "memberships");

            migrationBuilder.DropTable(
                name: "subscription_statuses");

            migrationBuilder.DropTable(
                name: "service_categories");

            migrationBuilder.DropTable(
                name: "activities");

            migrationBuilder.DropTable(
                name: "activity_statuses");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "organizations");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
