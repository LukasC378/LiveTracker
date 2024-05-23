using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RaceTrackerAPI.Migrations
{
    /// <inheritdoc />
    public partial class ColletionRelationsKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Team_Active",
                table: "Team");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "Team");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "TeamCollection",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "DriverCollection",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TeamCollection",
                table: "TeamCollection",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DriverCollection",
                table: "DriverCollection",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TeamCollection",
                table: "TeamCollection");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DriverCollection",
                table: "DriverCollection");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TeamCollection");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "DriverCollection");

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Team",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Team_Active",
                table: "Team",
                column: "Active");
        }
    }
}
