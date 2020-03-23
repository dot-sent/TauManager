using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TauManager.Migrations.TauDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class UserData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Player",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
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
                    table.PrimaryKey("PK_Player", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Player");
        }
    }
}
