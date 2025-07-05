using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateOrderEntityV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageConfirmed",
                table: "order",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShipperId",
                table: "order",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageConfirmed",
                table: "order");

            migrationBuilder.DropColumn(
                name: "ShipperId",
                table: "order");
        }
    }
}
