using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantHub.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddWateringEventTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastWateredUtc",
                table: "Plants");

            migrationBuilder.CreateTable(
                name: "WateringEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlantId = table.Column<int>(type: "INTEGER", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WateringEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WateringEvents_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WateringEvents_PlantId",
                table: "WateringEvents",
                column: "PlantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WateringEvents");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWateredUtc",
                table: "Plants",
                type: "TEXT",
                nullable: true);
        }
    }
}
