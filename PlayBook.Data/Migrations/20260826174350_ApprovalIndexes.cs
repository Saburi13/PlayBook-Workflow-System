using Microsoft.EntityFrameworkCore.Migrations;
using PlayBook.Data.Context;

#nullable disable

namespace PlayBook.Data.Migrations
{
    /// <inheritdoc />
    public partial class ApprovalIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Approvals_ApproverEmployeeId",
                table: "Approvals");

            migrationBuilder.DropIndex(
                name: "IX_Approvals_ProposalId",
                table: "Approvals");

            migrationBuilder.CreateIndex(
                name: "IX_Approvals_ApproverEmployeeId_Status",
                table: "Approvals",
                columns: new[] { "ApproverEmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Approvals_ProposalId_Status",
                table: "Approvals",
                columns: new[] { "ProposalId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Approvals_ApproverEmployeeId_Status",
                table: "Approvals");

            migrationBuilder.DropIndex(
                name: "IX_Approvals_ProposalId_Status",
                table: "Approvals");

            migrationBuilder.CreateIndex(
                name: "IX_Approvals_ApproverEmployeeId",
                table: "Approvals",
                column: "ApproverEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Approvals_ProposalId",
                table: "Approvals",
                column: "ProposalId");
        }
    }
}
