using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantHub.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddLastWateringReminder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastWateringReminderUtc",
                table: "Plants",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastWateringReminderUtc",
                table: "Plants");
        }
    }
}
