using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiCore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureIdentityRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationMembers_AspNetUsers_UserId",
                table: "OrganizationMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_AspNetUsers_OwnerUserId",
                table: "Organizations",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationMembers_AspNetUsers_UserId",
                table: "OrganizationMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_AspNetUsers_OwnerUserId",
                table: "Organizations");
        }
    }
}
