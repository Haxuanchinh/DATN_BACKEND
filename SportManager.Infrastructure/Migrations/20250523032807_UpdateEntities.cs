using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "order",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "VoucherId",
                table: "order",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_VoucherId",
                table: "order",
                column: "VoucherId");

            migrationBuilder.AddForeignKey(
                name: "FK_order_voucher_VoucherId",
                table: "order",
                column: "VoucherId",
                principalTable: "voucher",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_voucher_VoucherId",
                table: "order");

            migrationBuilder.DropIndex(
                name: "IX_order_VoucherId",
                table: "order");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "order");

            migrationBuilder.DropColumn(
                name: "VoucherId",
                table: "order");
        }
    }
}
