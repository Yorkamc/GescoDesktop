using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gesco.Desktop.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithOptimization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "activity_statuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
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
                    table.PrimaryKey("PK_activity_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inventory_movement_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
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
                name: "memberships",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
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
                    id = table.Column<int>(type: "INTEGER", nullable: false)
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
                    id = table.Column<int>(type: "INTEGER", nullable: false)
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
                    id = table.Column<int>(type: "INTEGER", nullable: false)
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
                    id = table.Column<int>(type: "INTEGER", nullable: false)
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
                    id = table.Column<int>(type: "INTEGER", nullable: false)
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
                    id = table.Column<int>(type: "INTEGER", nullable: false)
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "system_configuration",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
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
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    cedula = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    email_verified_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    password = table.Column<string>(type: "TEXT", nullable: false),
                    full_name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    phone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    first_login = table.Column<bool>(type: "INTEGER", nullable: false),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    role_id = table.Column<int>(type: "INTEGER", nullable: false),
                    first_login_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    last_login_at = table.Column<DateTime>(type: "TEXT", nullable: true),
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
                    table.PrimaryKey("PK_users", x => x.cedula);
                    table.ForeignKey(
                        name: "FK_users_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    membership_id = table.Column<int>(type: "INTEGER", nullable: false),
                    subscription_status_id = table.Column<int>(type: "INTEGER", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_subscriptions_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_subscriptions_subscription_statuses_subscription_status_id",
                        column: x => x.subscription_status_id,
                        principalTable: "subscription_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "activities",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    start_date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    end_date = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    end_time = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    activity_status_id = table.Column<int>(type: "INTEGER", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_activities_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_activities_users_manager_user_id",
                        column: x => x.manager_user_id,
                        principalTable: "users",
                        principalColumn: "cedula");
                });

            migrationBuilder.CreateTable(
                name: "api_activity_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
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
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_api_activity_logs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "cedula");
                });

            migrationBuilder.CreateTable(
                name: "desktop_clients",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_desktop_clients_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "cedula",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    notification_type_id = table.Column<int>(type: "INTEGER", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notifications_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "cedula");
                });

            migrationBuilder.CreateTable(
                name: "oauth_access_tokens",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
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
                        principalColumn: "cedula");
                });

            migrationBuilder.CreateTable(
                name: "activation_keys",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    activation_code = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    subscription_id = table.Column<int>(type: "INTEGER", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_activation_keys_users_generated_by",
                        column: x => x.generated_by,
                        principalTable: "users",
                        principalColumn: "cedula");
                    table.ForeignKey(
                        name: "FK_activation_keys_users_revoked_by",
                        column: x => x.revoked_by,
                        principalTable: "users",
                        principalColumn: "cedula");
                    table.ForeignKey(
                        name: "FK_activation_keys_users_used_by_user_id",
                        column: x => x.used_by_user_id,
                        principalTable: "users",
                        principalColumn: "cedula");
                });

            migrationBuilder.CreateTable(
                name: "activity_categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    activity_id = table.Column<int>(type: "INTEGER", nullable: false),
                    service_category_id = table.Column<int>(type: "INTEGER", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_activity_categories_service_categories_service_category_id",
                        column: x => x.service_category_id,
                        principalTable: "service_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "activity_closures",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    activity_id = table.Column<int>(type: "INTEGER", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_activity_closures_users_closed_by",
                        column: x => x.closed_by,
                        principalTable: "users",
                        principalColumn: "cedula");
                    table.ForeignKey(
                        name: "FK_activity_closures_users_supervised_by",
                        column: x => x.supervised_by,
                        principalTable: "users",
                        principalColumn: "cedula");
                });

            migrationBuilder.CreateTable(
                name: "cash_registers",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    activity_id = table.Column<int>(type: "INTEGER", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cash_registers_users_operator_user_id",
                        column: x => x.operator_user_id,
                        principalTable: "users",
                        principalColumn: "cedula");
                    table.ForeignKey(
                        name: "FK_cash_registers_users_supervisor_user_id",
                        column: x => x.supervisor_user_id,
                        principalTable: "users",
                        principalColumn: "cedula");
                });

            migrationBuilder.CreateTable(
                name: "sales_combos",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    activity_id = table.Column<int>(type: "INTEGER", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sync_queue",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    client_id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    affected_table = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    record_id = table.Column<long>(type: "INTEGER", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sync_queue_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sync_versions",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    table_name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    record_id = table.Column<long>(type: "INTEGER", nullable: false),
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
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_sync_versions_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sync_versions_users_changed_by_user",
                        column: x => x.changed_by_user,
                        principalTable: "users",
                        principalColumn: "cedula");
                });

            migrationBuilder.CreateTable(
                name: "oauth_refresh_tokens",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "activation_history",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    activation_key_id = table.Column<int>(type: "INTEGER", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_activation_history_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_activation_history_users_activated_by_user_id",
                        column: x => x.activated_by_user_id,
                        principalTable: "users",
                        principalColumn: "cedula");
                    table.ForeignKey(
                        name: "FK_activation_history_users_deactivated_by",
                        column: x => x.deactivated_by,
                        principalTable: "users",
                        principalColumn: "cedula");
                });

            migrationBuilder.CreateTable(
                name: "category_products",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    activity_category_id = table.Column<int>(type: "INTEGER", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cash_register_closures",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    cash_register_id = table.Column<int>(type: "INTEGER", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cash_register_closures_users_closed_by",
                        column: x => x.closed_by,
                        principalTable: "users",
                        principalColumn: "cedula");
                    table.ForeignKey(
                        name: "FK_cash_register_closures_users_supervised_by",
                        column: x => x.supervised_by,
                        principalTable: "users",
                        principalColumn: "cedula");
                });

            migrationBuilder.CreateTable(
                name: "sales_transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    cash_register_id = table.Column<int>(type: "INTEGER", nullable: false),
                    transaction_number = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    invoice_number = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    sales_status_id = table.Column<int>(type: "INTEGER", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sales_transactions_sales_statuses_sales_status_id",
                        column: x => x.sales_status_id,
                        principalTable: "sales_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "combo_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    combo_id = table.Column<int>(type: "INTEGER", nullable: false),
                    product_id = table.Column<int>(type: "INTEGER", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_combo_items_sales_combos_combo_id",
                        column: x => x.combo_id,
                        principalTable: "sales_combos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inventory_movements",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    product_id = table.Column<int>(type: "INTEGER", nullable: false),
                    movement_type_id = table.Column<int>(type: "INTEGER", nullable: false),
                    quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    previous_quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    new_quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    unit_cost = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    total_value = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    sales_transaction_id = table.Column<int>(type: "INTEGER", nullable: true),
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_inventory_movements_inventory_movement_types_movement_type_id",
                        column: x => x.movement_type_id,
                        principalTable: "inventory_movement_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_inventory_movements_sales_transactions_sales_transaction_id",
                        column: x => x.sales_transaction_id,
                        principalTable: "sales_transactions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_inventory_movements_users_authorized_by",
                        column: x => x.authorized_by,
                        principalTable: "users",
                        principalColumn: "cedula");
                    table.ForeignKey(
                        name: "FK_inventory_movements_users_performed_by",
                        column: x => x.performed_by,
                        principalTable: "users",
                        principalColumn: "cedula");
                });

            migrationBuilder.CreateTable(
                name: "transaction_details",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    sales_transaction_id = table.Column<int>(type: "INTEGER", nullable: false),
                    product_id = table.Column<int>(type: "INTEGER", nullable: true),
                    combo_id = table.Column<int>(type: "INTEGER", nullable: true),
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
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_transaction_details_sales_combos_combo_id",
                        column: x => x.combo_id,
                        principalTable: "sales_combos",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_transaction_details_sales_transactions_sales_transaction_id",
                        column: x => x.sales_transaction_id,
                        principalTable: "sales_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transaction_payments",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    sales_transaction_id = table.Column<int>(type: "INTEGER", nullable: false),
                    payment_method_id = table.Column<int>(type: "INTEGER", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_transaction_payments_sales_transactions_sales_transaction_id",
                        column: x => x.sales_transaction_id,
                        principalTable: "sales_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_transaction_payments_users_processed_by",
                        column: x => x.processed_by,
                        principalTable: "users",
                        principalColumn: "cedula",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_api_activity_logs_organization_id",
                table: "api_activity_logs",
                column: "organization_id");

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
                name: "IX_notifications_notification_type_id",
                table: "notifications",
                column: "notification_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_organization_id",
                table: "notifications",
                column: "organization_id");

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
                name: "IX_sync_queue_client_id",
                table: "sync_queue",
                column: "client_id");

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
                name: "IX_users_organization_id",
                table: "users",
                column: "organization_id");

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
