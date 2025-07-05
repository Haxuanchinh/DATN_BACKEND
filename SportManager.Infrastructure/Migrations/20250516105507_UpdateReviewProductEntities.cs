using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReviewProductEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "Images",
                table: "product_review",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VariantId",
                table: "product_review",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "product_review_comment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_review_comment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_review_comment_customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_review_comment_product_review_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "product_review",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_review_comment_product_review_comment_ParentComment~",
                        column: x => x.ParentCommentId,
                        principalTable: "product_review_comment",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_review_VariantId",
                table: "product_review",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_product_review_comment_CustomerId",
                table: "product_review_comment",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_product_review_comment_ParentCommentId",
                table: "product_review_comment",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_product_review_comment_ReviewId",
                table: "product_review_comment",
                column: "ReviewId");

            migrationBuilder.AddForeignKey(
                name: "FK_product_review_product_variant_VariantId",
                table: "product_review",
                column: "VariantId",
                principalTable: "product_variant",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_review_product_variant_VariantId",
                table: "product_review");

            migrationBuilder.DropTable(
                name: "product_review_comment");

            migrationBuilder.DropIndex(
                name: "IX_product_review_VariantId",
                table: "product_review");

            migrationBuilder.DropColumn(
                name: "Images",
                table: "product_review");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "product_review");
        }
    }
}
