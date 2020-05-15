using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TauManager.Migrations.TauDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class AddedNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    SendAfter = table.Column<DateTime>(nullable: false),
                    RecipientId = table.Column<int>(nullable: false),
                    Status = table.Column<byte>(nullable: false),
                    RelatedId = table.Column<long>(nullable: true),
                    RetryCount = table.Column<byte>(nullable: false),
                    Kind = table.Column<byte>(nullable: false),
                    Audit = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notification_Player_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_RecipientId",
                table: "Notification",
                column: "RecipientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notification");
        }
    }
}
