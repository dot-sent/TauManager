using Microsoft.EntityFrameworkCore.Migrations;

namespace TauManager.Migrations.TauDb
{
    public partial class AddedPlayerActive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Player",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                table: "Player");
        }
    }
}
