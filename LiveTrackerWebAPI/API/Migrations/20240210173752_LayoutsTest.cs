using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceTrackerAPI.Migrations
{
    /// <inheritdoc />
    public partial class LayoutsTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Distance",
                table: "Layout");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Layout");

            migrationBuilder.AlterColumn<string>(
                name: "GeoJson",
                table: "Session",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "LayoutId",
                table: "Session",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Layout",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "Active",
                table: "Layout");

            migrationBuilder.AlterColumn<string>(
                name: "GeoJson",
                table: "Session",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Distance",
                table: "Layout",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Layout",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
