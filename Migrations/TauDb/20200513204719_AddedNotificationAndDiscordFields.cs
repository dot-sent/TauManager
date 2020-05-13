using Microsoft.EntityFrameworkCore.Migrations;

namespace TauManager.Migrations.TauDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class AddedNotificationAndDiscordFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiscordLogin",
                table: "Player",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NotificationSettings",
                table: "Player",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscordLogin",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "NotificationSettings",
                table: "Player");
        }
    }
}
