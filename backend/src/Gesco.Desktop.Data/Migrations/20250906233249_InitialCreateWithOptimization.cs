using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

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
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
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
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    requires_justification = table.Column<bool>(type: "INTEGER", nullable: false),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
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
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    monthly_price = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    annual_price = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    user_limit = table.Column<int>(type: "INTEGER", nullable: false),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memberships", x => x.id);
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
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_methods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    requires_reference = table.Column<bool>(type: "INTEGER", nullable: false),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
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
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
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
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
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
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    allows_system_usage = table.Column<bool>(type: "INTEGER", nullable: false),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "system_configurations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
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
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_configurations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "activities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    start_date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    end_date = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    end_time = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    activity_status_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    manager_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "service_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
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
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    email_verified_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    password = table.Column<string>(type: "TEXT", nullable: false),
                    full_name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    phone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    first_login = table.Column<bool>(type: "INTEGER", nullable: false),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    role_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    first_login_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    last_login_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
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
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    organization_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    membership_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    subscription_status_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    start_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    expiration_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    grace_period_end = table.Column<DateTime>(type: "TEXT", nullable: false),
                    cancellation_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
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
                name: "activity_closures",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    activity_id = table.Column<Guid>(type: "TEXT", nullable: false),
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
                    closed_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    supervised_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    observations = table.Column<string>(type: "TEXT", nullable: true),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "cash_registers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    activity_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    register_number = table.Column<int>(type: "INTEGER", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    is_open = table.Column<bool>(type: "INTEGER", nullable: false),
                    opened_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    closed_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    operator_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    supervisor_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "sales_combos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    activity_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    combo_price = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
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
                name: "activity_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    activity_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    service_category_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
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
                name: "activation_keys",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    activation_code = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    subscription_id = table.Column<Guid>(type: "TEXT", nullable: false),
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
                    used_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    activation_ip = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    generated_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    revoked_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    revocation_reason = table.Column<string>(type: "TEXT", nullable: true),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
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
                });

            migrationBuilder.CreateTable(
                name: "cash_register_closures",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    cash_register_id = table.Column<Guid>(type: "TEXT", nullable: false),
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
                    closed_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    supervised_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    observations = table.Column<string>(type: "TEXT", nullable: true),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "sales_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    cash_register_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    transaction_number = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    invoice_number = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    sales_status_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    transaction_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    total_amount = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
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
                name: "category_products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    activity_category_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    unit_price = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    initial_quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    current_quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    alert_quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
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
                name: "transaction_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    sales_transaction_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    payment_method_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    amount = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    reference = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    processed_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    processed_by = table.Column<Guid>(type: "TEXT", nullable: false),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "combo_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    combo_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    product_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
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
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    product_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    movement_type_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    previous_quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    new_quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    unit_cost = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    total_value = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    sales_transaction_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    justification = table.Column<string>(type: "TEXT", nullable: true),
                    performed_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    authorized_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    movement_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "transaction_details",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    sales_transaction_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    product_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    combo_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    unit_price = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    total_amount = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    is_combo = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_by = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    sync_version = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    integrity_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
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

            migrationBuilder.InsertData(
                table: "activity_statuses",
                columns: new[] { "id", "active", "created_at", "created_by", "description", "name", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("317dce9b-92ae-4365-b267-d4e086a1a7ee"), true, new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8478), null, "Activity not started", "Not Started", null, null },
                    { new Guid("b73028b1-9475-4faf-a006-dd6c0fc0ec9a"), true, new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8483), null, "Activity completed", "Completed", null, null },
                    { new Guid("caf73678-6d4f-4889-81f3-2aaa8c249cf3"), true, new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8480), null, "Activity in development", "In Progress", null, null },
                    { new Guid("cfc5266d-9c7c-4182-a1e6-5810e53754a5"), true, new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8487), null, "Activity cancelled", "Cancelled", null, null }
                });

            migrationBuilder.InsertData(
                table: "inventory_movement_types",
                columns: new[] { "id", "active", "created_at", "created_by", "description", "name", "requires_justification", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("2d8ea2ae-3c87-41c5-a55b-2694590506dc"), true, new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8587), null, "Stock out by product sale", "Sale", false, null, null },
                    { new Guid("413d6494-6f72-4fd7-a940-d076729fcbb9"), true, new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8589), null, "Inventory adjustment for differences", "Adjustment", true, null, null },
                    { new Guid("e925a53f-12d0-4f7f-b6aa-9384a8d58d11"), true, new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8582), null, "Merchandise entry to inventory", "Stock In", false, null, null }
                });

            migrationBuilder.InsertData(
                table: "organizations",
                columns: new[] { "id", "active", "address", "contact_email", "contact_phone", "created_at", "created_by", "integrity_hash", "last_sync", "name", "purchaser_name", "sync_version", "updated_at", "updated_by" },
                values: new object[] { new Guid("cf17936f-7a72-4626-b84f-3e889da46534"), true, "San José, Costa Rica", "demo@gesco.com", "2222-2222", new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(7968), null, null, null, "Demo Organization", "Demo Administrator", 1L, null, null });

            migrationBuilder.InsertData(
                table: "payment_methods",
                columns: new[] { "id", "active", "created_at", "created_by", "name", "requires_reference", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("2d443ee3-ebc4-41a7-b8dc-ae0c451fbc44"), true, new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8550), null, "Card", true, null, null },
                    { new Guid("8cfefaab-6bd3-40b2-a180-544deaf01fe3"), true, new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8553), null, "SINPE Mobile", true, null, null },
                    { new Guid("a1670f87-4239-4a98-9b4e-2d4837593771"), true, new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8547), null, "Cash", false, null, null }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "active", "created_at", "created_by", "description", "name", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("49dab6c2-3570-4377-a39d-65f1fa234b36"), true, new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8409), null, "Activity supervision", "Supervisor", null, null },
                    { new Guid("66990a4c-4c45-414a-88c8-639efe1f4bde"), true, new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8404), null, "Full system access", "Administrator", null, null },
                    { new Guid("c58a2d28-8554-4567-9e49-bbb46a2f6c20"), true, new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8407), null, "Sales and cash register access", "Salesperson", null, null }
                });

            migrationBuilder.InsertData(
                table: "sales_statuses",
                columns: new[] { "id", "active", "created_at", "created_by", "description", "name", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("8cf0b8ba-61d9-41a6-98e8-5db97125afff"), true, new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8520), null, "Sale completed successfully", "Completed", null, null },
                    { new Guid("e44e0b65-07ff-4dad-979c-0d66a987eb7e"), true, new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8522), null, "Sale cancelled", "Cancelled", null, null },
                    { new Guid("fbda1583-262a-4f60-a897-f65b9932ee97"), true, new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8517), null, "Sale pending processing", "Pending", null, null }
                });

            migrationBuilder.InsertData(
                table: "subscription_statuses",
                columns: new[] { "id", "active", "allows_system_usage", "created_at", "created_by", "description", "name", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("23b94cb0-3409-4aed-93de-61c775fc7382"), true, false, new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8621), null, "Suspended subscription", "Suspended", null, null },
                    { new Guid("a7e76538-9b62-4c39-9797-cb6355e0eb05"), true, false, new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8626), null, "Cancelled subscription", "Cancelled", null, null },
                    { new Guid("cbb31f95-7338-46d4-be71-43c04541c5d3"), true, true, new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8618), null, "Active subscription", "Active", null, null }
                });

            migrationBuilder.InsertData(
                table: "system_configurations",
                columns: new[] { "id", "access_level", "allowed_values", "category", "created_at", "created_by", "data_type", "description", "display_order", "environment", "is_editable", "is_sensitive", "key", "max_value", "min_value", "restart_required", "updated_at", "updated_by", "validation_pattern", "value" },
                values: new object[,]
                {
                    { new Guid("12851d93-e880-44fa-8491-a18cda26f6c0"), "admin", null, "system", new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8651), null, "string", "System version", 0, "all", false, false, "system.version", null, null, false, null, null, null, "1.0.0" },
                    { new Guid("94df5d9d-ddfc-47e3-a81c-07dca79196bb"), "admin", null, "license", new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8657), null, "int", "License check interval in days", 0, "all", true, false, "license.check_interval_days", null, null, false, null, null, null, "7" },
                    { new Guid("b137f2f6-29e4-4cfa-abb4-67db04a1e522"), "admin", null, "backup", new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8654), null, "int", "Backup interval in hours", 0, "all", true, false, "backup.interval_hours", null, null, false, null, null, null, "6" }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "active", "created_at", "created_by", "email", "email_verified_at", "first_login", "first_login_at", "full_name", "integrity_hash", "last_login_at", "last_sync", "organization_id", "password", "phone", "role_id", "sync_version", "updated_at", "updated_by", "username" },
                values: new object[] { new Guid("17f458d8-ca21-439d-bebc-7b0666e1afbb"), true, new DateTime(2025, 9, 6, 23, 32, 49, 151, DateTimeKind.Utc).AddTicks(8448), null, "admin@gesco.com", null, true, null, "System Administrator", null, null, null, new Guid("cf17936f-7a72-4626-b84f-3e889da46534"), "$2a$12$6nybiEVKavFp/iZhsQrSLuNIhhAnRx2STs6Fmzj.BCF4gUAwMtCV6", "8888-8888", new Guid("66990a4c-4c45-414a-88c8-639efe1f4bde"), 1L, null, null, "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_activation_keys_subscription_id",
                table: "activation_keys",
                column: "subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_activities_activity_status_id",
                table: "activities",
                column: "activity_status_id");

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
                name: "IX_cash_register_closures_cash_register_id",
                table: "cash_register_closures",
                column: "cash_register_id");

            migrationBuilder.CreateIndex(
                name: "IX_cash_registers_activity_id",
                table: "cash_registers",
                column: "activity_id");

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
                name: "IX_inventory_movements_movement_type_id",
                table: "inventory_movements",
                column: "movement_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_movements_product_id",
                table: "inventory_movements",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_movements_sales_transaction_id",
                table: "inventory_movements",
                column: "sales_transaction_id");

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
                name: "activation_keys");

            migrationBuilder.DropTable(
                name: "activity_closures");

            migrationBuilder.DropTable(
                name: "cash_register_closures");

            migrationBuilder.DropTable(
                name: "combo_items");

            migrationBuilder.DropTable(
                name: "inventory_movements");

            migrationBuilder.DropTable(
                name: "system_configurations");

            migrationBuilder.DropTable(
                name: "transaction_details");

            migrationBuilder.DropTable(
                name: "transaction_payments");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "inventory_movement_types");

            migrationBuilder.DropTable(
                name: "category_products");

            migrationBuilder.DropTable(
                name: "sales_combos");

            migrationBuilder.DropTable(
                name: "payment_methods");

            migrationBuilder.DropTable(
                name: "sales_transactions");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "memberships");

            migrationBuilder.DropTable(
                name: "subscription_statuses");

            migrationBuilder.DropTable(
                name: "activity_categories");

            migrationBuilder.DropTable(
                name: "cash_registers");

            migrationBuilder.DropTable(
                name: "sales_statuses");

            migrationBuilder.DropTable(
                name: "service_categories");

            migrationBuilder.DropTable(
                name: "activities");

            migrationBuilder.DropTable(
                name: "activity_statuses");

            migrationBuilder.DropTable(
                name: "organizations");
        }
    }
}
