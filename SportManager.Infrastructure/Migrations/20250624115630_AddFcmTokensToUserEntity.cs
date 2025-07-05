using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFcmTokensToUserEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FcmToken",
                table: "user");

            migrationBuilder.AddColumn<string[]>(
                name: "FcmTokens",
                table: "user",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FcmTokens",
                table: "user");

            migrationBuilder.AddColumn<string>(
                name: "FcmToken",
                table: "user",
                type: "text",
                nullable: true);
        }
    }
}
