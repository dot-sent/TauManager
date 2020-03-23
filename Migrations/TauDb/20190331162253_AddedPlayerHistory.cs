using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TauManager.Migrations.TauDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class AddedPlayerHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerHistory",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    PlayerId = table.Column<int>(nullable: false),
                    RecordedAt = table.Column<DateTime>(nullable: false),
                    Level = table.Column<decimal>(nullable: false),
                    Strength = table.Column<decimal>(nullable: false),
                    Stamina = table.Column<decimal>(nullable: false),
                    Agility = table.Column<decimal>(nullable: false),
                    Intelligence = table.Column<decimal>(nullable: false),
                    Social = table.Column<decimal>(nullable: false),
                    Wallet = table.Column<decimal>(nullable: false),
                    Bank = table.Column<decimal>(nullable: false),
                    Bonds = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerHistory_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerHistory_PlayerId",
                table: "PlayerHistory",
                column: "PlayerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerHistory");
        }
    }
}
