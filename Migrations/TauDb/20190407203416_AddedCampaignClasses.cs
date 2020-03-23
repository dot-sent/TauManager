using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TauManager.Migrations.TauDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class AddedCampaignClasses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Campaign",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    UTCDateTime = table.Column<DateTime>(nullable: true),
                    ManagerId = table.Column<int>(nullable: true),
                    Status = table.Column<byte>(nullable: false),
                    Station = table.Column<string>(nullable: true),
                    Difficulty = table.Column<byte>(nullable: true),
                    Tiers = table.Column<int>(nullable: true),
                    Comments = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaign", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Campaign_Player_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: false),
                    Type = table.Column<byte>(nullable: false),
                    Tier = table.Column<int>(nullable: false),
                    Weight = table.Column<decimal>(nullable: false),
                    Price = table.Column<decimal>(nullable: false),
                    Rarity = table.Column<byte>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    ImageUrl = table.Column<string>(nullable: true),
                    ItemUrl = table.Column<string>(nullable: true),
                    Accuracy = table.Column<decimal>(nullable: true),
                    HandToHand = table.Column<bool>(nullable: true),
                    WeaponRange = table.Column<byte>(nullable: true),
                    WeaponType = table.Column<byte>(nullable: true),
                    Piercing = table.Column<decimal>(nullable: true),
                    Impact = table.Column<decimal>(nullable: true),
                    Energy = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignAttendance",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    CampaignId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignAttendance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignAttendance_Campaign_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaign",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignAttendance_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignSignup",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    CampaignId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false),
                    Attending = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignSignup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignSignup_Campaign_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaign",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignSignup_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignLoot",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    CampaignId = table.Column<int>(nullable: false),
                    ItemId = table.Column<int>(nullable: false),
                    HolderId = table.Column<int>(nullable: true),
                    Status = table.Column<byte>(nullable: false),
                    Comments = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignLoot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignLoot_Campaign_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaign",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignLoot_Player_HolderId",
                        column: x => x.HolderId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignLoot_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LootRequest",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    RequestedForId = table.Column<int>(nullable: false),
                    RequestedById = table.Column<int>(nullable: false),
                    LootId = table.Column<int>(nullable: false),
                    SpecialOfferDescription = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LootRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LootRequest_CampaignLoot_LootId",
                        column: x => x.LootId,
                        principalTable: "CampaignLoot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LootRequest_Player_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LootRequest_Player_RequestedForId",
                        column: x => x.RequestedForId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerListPositionHistory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    LootRequestId = table.Column<int>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerListPositionHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerListPositionHistory_LootRequest_LootRequestId",
                        column: x => x.LootRequestId,
                        principalTable: "LootRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerListPositionHistory_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Campaign_ManagerId",
                table: "Campaign",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignAttendance_CampaignId",
                table: "CampaignAttendance",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignAttendance_PlayerId",
                table: "CampaignAttendance",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignLoot_CampaignId",
                table: "CampaignLoot",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignLoot_HolderId",
                table: "CampaignLoot",
                column: "HolderId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignLoot_ItemId",
                table: "CampaignLoot",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignSignup_CampaignId",
                table: "CampaignSignup",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignSignup_PlayerId",
                table: "CampaignSignup",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_LootRequest_LootId",
                table: "LootRequest",
                column: "LootId");

            migrationBuilder.CreateIndex(
                name: "IX_LootRequest_RequestedById",
                table: "LootRequest",
                column: "RequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_LootRequest_RequestedForId",
                table: "LootRequest",
                column: "RequestedForId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerListPositionHistory_LootRequestId",
                table: "PlayerListPositionHistory",
                column: "LootRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerListPositionHistory_PlayerId",
                table: "PlayerListPositionHistory",
                column: "PlayerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignAttendance");

            migrationBuilder.DropTable(
                name: "CampaignSignup");

            migrationBuilder.DropTable(
                name: "PlayerListPositionHistory");

            migrationBuilder.DropTable(
                name: "LootRequest");

            migrationBuilder.DropTable(
                name: "CampaignLoot");

            migrationBuilder.DropTable(
                name: "Campaign");

            migrationBuilder.DropTable(
                name: "Item");
        }
    }
}
