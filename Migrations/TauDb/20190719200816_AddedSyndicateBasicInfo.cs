using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TauManager.Migrations.TauDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class AddedSyndicateBasicInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SyndicateId",
                table: "Player",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SyndicateId",
                table: "Campaign",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Syndicate",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Tag = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Syndicate", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Player_SyndicateId",
                table: "Player",
                column: "SyndicateId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaign_SyndicateId",
                table: "Campaign",
                column: "SyndicateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaign_Syndicate_SyndicateId",
                table: "Campaign",
                column: "SyndicateId",
                principalTable: "Syndicate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Player_Syndicate_SyndicateId",
                table: "Player",
                column: "SyndicateId",
                principalTable: "Syndicate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaign_Syndicate_SyndicateId",
                table: "Campaign");

            migrationBuilder.DropForeignKey(
                name: "FK_Player_Syndicate_SyndicateId",
                table: "Player");

            migrationBuilder.DropTable(
                name: "Syndicate");

            migrationBuilder.DropIndex(
                name: "IX_Player_SyndicateId",
                table: "Player");

            migrationBuilder.DropIndex(
                name: "IX_Campaign_SyndicateId",
                table: "Campaign");

            migrationBuilder.DropColumn(
                name: "SyndicateId",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "SyndicateId",
                table: "Campaign");
        }
    }
}
