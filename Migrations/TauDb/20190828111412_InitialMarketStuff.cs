using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TauManager.Migrations.TauDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class InitialMarketStuff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketAd",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AuthorId = table.Column<int>(nullable: false),
                    Type = table.Column<byte>(nullable: false),
                    PlacementDate = table.Column<DateTime>(nullable: false),
                    ExpiryDate = table.Column<DateTime>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    OfferType = table.Column<byte>(nullable: false),
                    RequestType = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketAd", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketAd_Player_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarketAdBundle",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Credits = table.Column<decimal>(nullable: false),
                    AdId = table.Column<int>(nullable: false),
                    Type = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketAdBundle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketAdBundle_MarketAd_AdId",
                        column: x => x.AdId,
                        principalTable: "MarketAd",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarketAdReaction",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AdId = table.Column<int>(nullable: false),
                    InterestedId = table.Column<int>(nullable: false),
                    ReactionDate = table.Column<DateTime>(nullable: false),
                    Message = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketAdReaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketAdReaction_MarketAd_AdId",
                        column: x => x.AdId,
                        principalTable: "MarketAd",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarketAdReaction_Player_InterestedId",
                        column: x => x.InterestedId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarketAdBundleItem",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    BundleId = table.Column<int>(nullable: false),
                    ItemId = table.Column<int>(nullable: false),
                    Quantity = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketAdBundleItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketAdBundleItem_MarketAdBundle_BundleId",
                        column: x => x.BundleId,
                        principalTable: "MarketAdBundle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarketAdBundleItem_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketAd_AuthorId",
                table: "MarketAd",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketAdBundle_AdId",
                table: "MarketAdBundle",
                column: "AdId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketAdBundleItem_BundleId",
                table: "MarketAdBundleItem",
                column: "BundleId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketAdBundleItem_ItemId",
                table: "MarketAdBundleItem",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketAdReaction_AdId",
                table: "MarketAdReaction",
                column: "AdId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketAdReaction_InterestedId",
                table: "MarketAdReaction",
                column: "InterestedId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketAdBundleItem");

            migrationBuilder.DropTable(
                name: "MarketAdReaction");

            migrationBuilder.DropTable(
                name: "MarketAdBundle");

            migrationBuilder.DropTable(
                name: "MarketAd");
        }
    }
}
