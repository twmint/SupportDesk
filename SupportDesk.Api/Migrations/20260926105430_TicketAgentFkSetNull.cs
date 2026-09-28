using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportDesk.Api.Migrations
{
    /// <inheritdoc />
    public partial class TicketAgentFkSetNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Agents_AgentId",
                table: "Tickets");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Agents_AgentId",
                table: "Tickets",
                column: "AgentId",
                principalTable: "Agents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Agents_AgentId",
                table: "Tickets");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Agents_AgentId",
                table: "Tickets",
                column: "AgentId",
                principalTable: "Agents",
                principalColumn: "Id");
        }
    }
}
