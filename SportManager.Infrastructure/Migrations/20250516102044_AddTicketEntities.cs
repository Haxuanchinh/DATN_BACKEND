using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "ticket_reply",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ImagesJson",
                table: "ticket_reply",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "customer_support_tictket",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "customer_support_tictket",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "customer_support_tictket",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "customer_statisfaction_rating",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_statisfaction_rating", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customer_statisfaction_rating_customer_support_tictket_Tick~",
                        column: x => x.TicketId,
                        principalTable: "customer_support_tictket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ticket_category",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticket_category", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_customer_support_tictket_CategoryId",
                table: "customer_support_tictket",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_statisfaction_rating_TicketId",
                table: "customer_statisfaction_rating",
                column: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_customer_support_tictket_ticket_category_CategoryId",
                table: "customer_support_tictket",
                column: "CategoryId",
                principalTable: "ticket_category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_customer_support_tictket_ticket_category_CategoryId",
                table: "customer_support_tictket");

            migrationBuilder.DropTable(
                name: "customer_statisfaction_rating");

            migrationBuilder.DropTable(
                name: "ticket_category");

            migrationBuilder.DropIndex(
                name: "IX_customer_support_tictket_CategoryId",
                table: "customer_support_tictket");

            migrationBuilder.DropColumn(
                name: "ImagesJson",
                table: "ticket_reply");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "customer_support_tictket");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "ticket_reply",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "customer_support_tictket",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "customer_support_tictket",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);
        }
    }
}
