using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayBook.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenewalRemindersAndPlanDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlanDurationMonths",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RenewalReminderId",
                table: "EngagementActivities",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionId",
                table: "EngagementActivities",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RenewalReminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OffsetDays = table.Column<int>(type: "int", nullable: false),
                    ReminderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenewalReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RenewalReminders_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EngagementActivities_RenewalReminderId",
                table: "EngagementActivities",
                column: "RenewalReminderId");

            migrationBuilder.CreateIndex(
                name: "IX_EngagementActivities_SubscriptionId",
                table: "EngagementActivities",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_RenewalReminders_SubscriptionId_OffsetDays",
                table: "RenewalReminders",
                columns: new[] { "SubscriptionId", "OffsetDays" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EngagementActivities_RenewalReminders_RenewalReminderId",
                table: "EngagementActivities",
                column: "RenewalReminderId",
                principalTable: "RenewalReminders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EngagementActivities_Subscriptions_SubscriptionId",
                table: "EngagementActivities",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EngagementActivities_RenewalReminders_RenewalReminderId",
                table: "EngagementActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_EngagementActivities_Subscriptions_SubscriptionId",
                table: "EngagementActivities");

            migrationBuilder.DropTable(
                name: "RenewalReminders");

            migrationBuilder.DropIndex(
                name: "IX_EngagementActivities_RenewalReminderId",
                table: "EngagementActivities");

            migrationBuilder.DropIndex(
                name: "IX_EngagementActivities_SubscriptionId",
                table: "EngagementActivities");

            migrationBuilder.DropColumn(
                name: "PlanDurationMonths",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RenewalReminderId",
                table: "EngagementActivities");

            migrationBuilder.DropColumn(
                name: "SubscriptionId",
                table: "EngagementActivities");
        }
    }
}
