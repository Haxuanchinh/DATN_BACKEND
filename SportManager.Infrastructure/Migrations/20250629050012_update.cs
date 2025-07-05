using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "user",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VoucherId",
                table: "product_variant",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessingDate",
                table: "order",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonRejectCancel",
                table: "order",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_variant_VoucherId",
                table: "product_variant",
                column: "VoucherId");

            migrationBuilder.AddForeignKey(
                name: "FK_product_variant_voucher_VoucherId",
                table: "product_variant",
                column: "VoucherId",
                principalTable: "voucher",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_variant_voucher_VoucherId",
                table: "product_variant");

            migrationBuilder.DropIndex(
                name: "IX_product_variant_VoucherId",
                table: "product_variant");

            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "user");

            migrationBuilder.DropColumn(
                name: "VoucherId",
                table: "product_variant");

            migrationBuilder.DropColumn(
                name: "ProcessingDate",
                table: "order");

            migrationBuilder.DropColumn(
                name: "ReasonRejectCancel",
                table: "order");
        }
    }
}
