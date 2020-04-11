using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TauManager.Migrations.TauDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class AddedSyndicateHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SyndicateHistory",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    SyndicateId = table.Column<int>(nullable: false),
                    Level = table.Column<decimal>(nullable: false),
                    Bonds = table.Column<int>(nullable: false),
                    Credits = table.Column<decimal>(nullable: false),
                    MembersCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyndicateHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyndicateHistory_Syndicate_SyndicateId",
                        column: x => x.SyndicateId,
                        principalTable: "Syndicate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SyndicateHistory_SyndicateId",
                table: "SyndicateHistory",
                column: "SyndicateId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyndicateHistory");
        }
    }
}
