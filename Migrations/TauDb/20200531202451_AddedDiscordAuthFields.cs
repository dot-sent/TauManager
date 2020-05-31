using Microsoft.EntityFrameworkCore.Migrations;

namespace TauManager.Migrations.TauDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class AddedDiscordAuthFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiscordAuthCode",
                table: "Player",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DiscordAuthConfirmed",
                table: "Player",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscordAuthCode",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "DiscordAuthConfirmed",
                table: "Player");
        }
    }
}
