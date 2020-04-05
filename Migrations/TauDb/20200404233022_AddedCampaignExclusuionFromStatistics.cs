using Microsoft.EntityFrameworkCore.Migrations;

namespace TauManager.Migrations.TauDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class AddedCampaignExclusuionFromStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ExcludeFromLeaderboards",
                table: "Campaign",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExcludeFromLeaderboards",
                table: "Campaign");
        }
    }
}
