using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RaceTrackerAPI.Migrations
{
    /// <inheritdoc />
    public partial class ColletionRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Driver_Collection_CollectionId",
                table: "Driver");

            migrationBuilder.DropForeignKey(
                name: "FK_Team_Collection_CollectionId",
                table: "Team");

            migrationBuilder.DropTable(
                name: "DriverSession");

            migrationBuilder.DropTable(
                name: "TeamSession");

            migrationBuilder.DropIndex(
                name: "IX_Team_CollectionId",
                table: "Team");

            migrationBuilder.DropIndex(
                name: "IX_Driver_Active",
                table: "Driver");

            migrationBuilder.DropIndex(
                name: "IX_Driver_CollectionId",
                table: "Driver");

            migrationBuilder.DropColumn(
                name: "CollectionId",
                table: "Team");

            migrationBuilder.DropColumn(
                name: "UseTeams",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "Driver");

            migrationBuilder.DropColumn(
                name: "CollectionId",
                table: "Driver");

            migrationBuilder.AddColumn<int>(
                name: "CollectionId",
                table: "Session",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DriverCollection",
                columns: table => new
                {
                    DriverId = table.Column<int>(type: "integer", nullable: false),
                    CollectionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_DriverCollection_Collection_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriverCollection_Driver_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamCollection",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "integer", nullable: false),
                    CollectionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_TeamCollection_Collection_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamCollection_Team_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Session_CollectionId",
                table: "Session",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverCollection_CollectionId",
                table: "DriverCollection",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverCollection_DriverId",
                table: "DriverCollection",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamCollection_CollectionId",
                table: "TeamCollection",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamCollection_TeamId",
                table: "TeamCollection",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverCollection");

            migrationBuilder.DropTable(
                name: "TeamCollection");

            migrationBuilder.DropIndex(
                name: "IX_Session_CollectionId",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "CollectionId",
                table: "Session");

            migrationBuilder.AddColumn<int>(
                name: "CollectionId",
                table: "Team",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "UseTeams",
                table: "Session",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Driver",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CollectionId",
                table: "Driver",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TeamSession",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Color = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SessionId = table.Column<int>(type: "integer", nullable: false),
                    TeamId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamSession_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DriverSession",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CarId = table.Column<Guid>(type: "uuid", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    SessionId = table.Column<int>(type: "integer", nullable: false),
                    TeamSessionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverSession_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriverSession_TeamSession_TeamSessionId",
                        column: x => x.TeamSessionId,
                        principalTable: "TeamSession",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Team_CollectionId",
                table: "Team",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Driver_Active",
                table: "Driver",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_Driver_CollectionId",
                table: "Driver",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverSession_SessionId",
                table: "DriverSession",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverSession_TeamSessionId",
                table: "DriverSession",
                column: "TeamSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamSession_SessionId",
                table: "TeamSession",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Driver_Collection_CollectionId",
                table: "Driver",
                column: "CollectionId",
                principalTable: "Collection",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Team_Collection_CollectionId",
                table: "Team",
                column: "CollectionId",
                principalTable: "Collection",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
