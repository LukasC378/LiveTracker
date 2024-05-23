using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceTrackerAPI.Migrations
{
    /// <inheritdoc />
    public partial class SessionLaps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Session_Layout_LayoutId",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "IX_Session_LayoutId",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "LayoutId",
                table: "Session");

            migrationBuilder.AddColumn<int>(
                name: "Laps",
                table: "Session",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Laps",
                table: "Session");

            migrationBuilder.AddColumn<int>(
                name: "LayoutId",
                table: "Session",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Session_LayoutId",
                table: "Session",
                column: "LayoutId");

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Layout_LayoutId",
                table: "Session",
                column: "LayoutId",
                principalTable: "Layout",
                principalColumn: "Id");
        }
    }
}
