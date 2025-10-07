using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrisonersDilemma.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Player1Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Player2Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PendingPlayer1Choice = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PendingPlayer2Choice = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Player1Score = table.Column<int>(type: "INTEGER", nullable: false),
                    Player2Score = table.Column<int>(type: "INTEGER", nullable: false),
                    Player1Choice = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Player2Choice = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Outcome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameResults_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameResults_GameSessionId",
                table: "GameResults",
                column: "GameSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameResults");

            migrationBuilder.DropTable(
                name: "GameSessions");
        }
    }
}
