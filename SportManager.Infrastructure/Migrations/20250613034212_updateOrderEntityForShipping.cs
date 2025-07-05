using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateOrderEntityForShipping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BillingAddress",
                table: "order",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BillingAddressId",
                table: "order",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CarrierId",
                table: "order",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Height",
                table: "order",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackingAddress",
                table: "order",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PackingAddressId",
                table: "order",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFee",
                table: "order",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitHeight",
                table: "order",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitWeight",
                table: "order",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitWidth",
                table: "order",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "order",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Width",
                table: "order",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "carrier",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    BaseShippingFee = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carrier", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "shipper",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LicenseNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VehicleType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VehicleNumber = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipper", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "shipping_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipping_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shipping_history_order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shipper_carrier",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipperId = table.Column<Guid>(type: "uuid", nullable: false),
                    CarrierId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipper_carrier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shipper_carrier_carrier_CarrierId",
                        column: x => x.CarrierId,
                        principalTable: "carrier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_shipper_carrier_shipper_ShipperId",
                        column: x => x.ShipperId,
                        principalTable: "shipper",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_order_CarrierId",
                table: "order",
                column: "CarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_order_ShipperId",
                table: "order",
                column: "ShipperId");

            migrationBuilder.CreateIndex(
                name: "IX_shipper_carrier_CarrierId",
                table: "shipper_carrier",
                column: "CarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_shipper_carrier_ShipperId",
                table: "shipper_carrier",
                column: "ShipperId");

            migrationBuilder.CreateIndex(
                name: "IX_shipping_history_OrderId",
                table: "shipping_history",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_order_carrier_CarrierId",
                table: "order",
                column: "CarrierId",
                principalTable: "carrier",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_order_shipper_ShipperId",
                table: "order",
                column: "ShipperId",
                principalTable: "shipper",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_carrier_CarrierId",
                table: "order");

            migrationBuilder.DropForeignKey(
                name: "FK_order_shipper_ShipperId",
                table: "order");

            migrationBuilder.DropTable(
                name: "shipper_carrier");

            migrationBuilder.DropTable(
                name: "shipping_history");

            migrationBuilder.DropTable(
                name: "carrier");

            migrationBuilder.DropTable(
                name: "shipper");

            migrationBuilder.DropIndex(
                name: "IX_order_CarrierId",
                table: "order");

            migrationBuilder.DropIndex(
                name: "IX_order_ShipperId",
                table: "order");

            migrationBuilder.DropColumn(
                name: "BillingAddress",
                table: "order");

            migrationBuilder.DropColumn(
                name: "BillingAddressId",
                table: "order");

            migrationBuilder.DropColumn(
                name: "CarrierId",
                table: "order");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "order");

            migrationBuilder.DropColumn(
                name: "PackingAddress",
                table: "order");

            migrationBuilder.DropColumn(
                name: "PackingAddressId",
                table: "order");

            migrationBuilder.DropColumn(
                name: "ShippingFee",
                table: "order");

            migrationBuilder.DropColumn(
                name: "UnitHeight",
                table: "order");

            migrationBuilder.DropColumn(
                name: "UnitWeight",
                table: "order");

            migrationBuilder.DropColumn(
                name: "UnitWidth",
                table: "order");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "order");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "order");
        }
    }
}
