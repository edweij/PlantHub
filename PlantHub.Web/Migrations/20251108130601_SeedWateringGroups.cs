using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PlantHub.Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedWateringGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "WateringGroups",
                columns: new[] { "Id", "CreatedUtc", "Description", "IntervalDaysSummer", "IntervalDaysWinter", "MaxDaysBetween", "MinDaysBetween", "Name", "SummerEndMonth", "SummerStartMonth", "UpdatedUtc" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Very drought-tolerant plants. Let soil dry completely. Typical: aloe, cacti, succulents.", 14, 21, 30, 14, "Succulents & Cacti", 9, 5, null },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Green foliage that prefers to dry slightly between waterings. Typical: monstera, pothos, umbrella plant.", 7, 10, 14, 7, "Drought-Tolerant Green Foliage", 9, 5, null },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Plants that like lightly and evenly moist soil. Typical: coleus (palettblad), indoor pelargonium.", 4, 6, 7, 3, "Thirsty Foliage & Flowering", 9, 5, null },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Citrus and olive in larger pots. Let dry lightly between waterings, adjust more between seasons.", 5, 14, 21, 3, "Mediterranean Trees", 9, 5, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "WateringGroups",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "WateringGroups",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "WateringGroups",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "WateringGroups",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
