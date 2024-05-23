using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RaceTrackerAPI.Migrations
{
    /// <inheritdoc />
    public partial class SessionResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Session_CancellationDate",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "CancellationDate",
                table: "Session");

            migrationBuilder.CreateTable(
                name: "SessionResult",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionId = table.Column<int>(type: "integer", nullable: false),
                    ResultJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionResult_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionResult_SessionId",
                table: "SessionResult",
                column: "SessionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionResult");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancellationDate",
                table: "Session",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Session_CancellationDate",
                table: "Session",
                column: "CancellationDate");
        }
    }
}
