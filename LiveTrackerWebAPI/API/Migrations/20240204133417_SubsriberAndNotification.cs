using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RaceTrackerAPI.Migrations
{
    /// <inheritdoc />
    public partial class SubsriberAndNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "Session",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "SessionNotification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionId = table.Column<int>(type: "integer", nullable: false),
                    InitNotification = table.Column<bool>(type: "boolean", nullable: false),
                    LoadNotification = table.Column<bool>(type: "boolean", nullable: false),
                    CancelNotification = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionNotification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionNotification_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriber",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    OrganizerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriber", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriber_User_OrganizerId",
                        column: x => x.OrganizerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscriber_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Session_CancellationDate",
                table: "Session",
                column: "CancellationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Session_ScheduledFrom",
                table: "Session",
                column: "ScheduledFrom");

            migrationBuilder.CreateIndex(
                name: "IX_Session_ScheduledTo",
                table: "Session",
                column: "ScheduledTo");

            migrationBuilder.CreateIndex(
                name: "IX_SessionNotification_SessionId",
                table: "SessionNotification",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriber_OrganizerId",
                table: "Subscriber",
                column: "OrganizerId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriber_UserId",
                table: "Subscriber",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionNotification");

            migrationBuilder.DropTable(
                name: "Subscriber");

            migrationBuilder.DropIndex(
                name: "IX_Session_CancellationDate",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "IX_Session_ScheduledFrom",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "IX_Session_ScheduledTo",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "Session");
        }
    }
}
