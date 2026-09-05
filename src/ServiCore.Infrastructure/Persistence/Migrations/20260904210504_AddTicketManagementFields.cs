using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiCore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketManagementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_AssignedAgentId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId",
                table: "Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_AssignedAgentId",
                table: "Tickets",
                columns: new[] { "OrganizationId", "AssignedAgentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_CategoryId",
                table: "Tickets",
                columns: new[] { "OrganizationId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_CustomerId",
                table: "Tickets",
                columns: new[] { "OrganizationId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_Priority",
                table: "Tickets",
                columns: new[] { "OrganizationId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_Status",
                table: "Tickets",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_TeamId",
                table: "Tickets",
                columns: new[] { "OrganizationId", "TeamId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_AssignedAgentId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_CategoryId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_CustomerId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_Priority",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_Status",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_TeamId",
                table: "Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AssignedAgentId",
                table: "Tickets",
                column: "AssignedAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId",
                table: "Tickets",
                column: "OrganizationId");
        }
    }
}
