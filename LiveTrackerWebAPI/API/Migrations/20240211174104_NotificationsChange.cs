using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceTrackerAPI.Migrations
{
    /// <inheritdoc />
    public partial class NotificationsChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelNotification",
                table: "SessionNotification");

            migrationBuilder.DropColumn(
                name: "InitNotification",
                table: "SessionNotification");

            migrationBuilder.DropColumn(
                name: "LoadNotification",
                table: "SessionNotification");

            migrationBuilder.AddColumn<int>(
                name: "NotificationType",
                table: "SessionNotification",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationType",
                table: "SessionNotification");

            migrationBuilder.AddColumn<bool>(
                name: "CancelNotification",
                table: "SessionNotification",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "InitNotification",
                table: "SessionNotification",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LoadNotification",
                table: "SessionNotification",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
