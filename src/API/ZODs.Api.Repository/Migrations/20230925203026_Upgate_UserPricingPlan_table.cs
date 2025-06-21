using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZODs.Api.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Upgate_UserPricingPlan_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserPricingPlans");

            migrationBuilder.DropColumn(
                name: "IsAutoRenewing",
                table: "UserPricingPlans");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "UserPricingPlans");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "UserPricingPlans");

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionStatusFormatted",
                table: "UserPricingPlans",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriptionStatusFormatted",
                table: "UserPricingPlans");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UserPricingPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAutoRenewing",
                table: "UserPricingPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "UserPricingPlans",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "UserPricingPlans",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
