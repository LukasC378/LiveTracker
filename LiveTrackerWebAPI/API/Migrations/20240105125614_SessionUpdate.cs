using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceTrackerAPI.Migrations
{
    /// <inheritdoc />
    public partial class SessionUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Active",
                table: "Session",
                newName: "Loaded");

            migrationBuilder.RenameIndex(
                name: "IX_Session_Active",
                table: "Session",
                newName: "IX_Session_Loaded");

            migrationBuilder.AddColumn<bool>(
                name: "Ended",
                table: "Session",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Session_Ended",
                table: "Session",
                column: "Ended");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Session_Ended",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "Ended",
                table: "Session");

            migrationBuilder.RenameColumn(
                name: "Loaded",
                table: "Session",
                newName: "Active");

            migrationBuilder.RenameIndex(
                name: "IX_Session_Loaded",
                table: "Session",
                newName: "IX_Session_Active");
        }
    }
}
