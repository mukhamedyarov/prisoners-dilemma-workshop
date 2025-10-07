using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrisonersDilemma.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Player1Id",
                table: "GameSessions",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "Player2Id",
                table: "GameSessions",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Player1Id",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "Player2Id",
                table: "GameSessions");
        }
    }
}
