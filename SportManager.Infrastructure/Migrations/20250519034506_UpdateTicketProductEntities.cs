using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTicketProductEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "ticket_reply",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ticket_reply_ParentId",
                table: "ticket_reply",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ticket_reply_ticket_reply_ParentId",
                table: "ticket_reply",
                column: "ParentId",
                principalTable: "ticket_reply",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ticket_reply_ticket_reply_ParentId",
                table: "ticket_reply");

            migrationBuilder.DropIndex(
                name: "IX_ticket_reply_ParentId",
                table: "ticket_reply");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "ticket_reply");
        }
    }
}
