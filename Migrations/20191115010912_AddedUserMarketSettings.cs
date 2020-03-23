using Microsoft.EntityFrameworkCore.Migrations;

namespace TauManager.Migrations
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class AddedUserMarketSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MarketIsViewPinned",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "MarketSort",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "MarketView",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MarketIsViewPinned",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MarketSort",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MarketView",
                table: "AspNetUsers");
        }
    }
}
