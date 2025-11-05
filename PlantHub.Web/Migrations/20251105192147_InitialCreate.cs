using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantHub.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WateringGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IntervalDaysSummer = table.Column<int>(type: "INTEGER", nullable: false),
                    IntervalDaysWinter = table.Column<int>(type: "INTEGER", nullable: false),
                    SummerStartMonth = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 5),
                    SummerEndMonth = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 9),
                    MinDaysBetween = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxDaysBetween = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WateringGroups", x => x.Id);
                    table.CheckConstraint("CK_WG_Interval_Summer", "IntervalDaysSummer >= 1");
                    table.CheckConstraint("CK_WG_Interval_Winter", "IntervalDaysWinter >= 1");
                    table.CheckConstraint("CK_WG_Max_Positive", "(MaxDaysBetween IS NULL) OR (MaxDaysBetween >= 1)");
                    table.CheckConstraint("CK_WG_MinLEMax", "(MinDaysBetween IS NULL) OR (MaxDaysBetween IS NULL) OR (MinDaysBetween <= MaxDaysBetween)");
                    table.CheckConstraint("CK_WG_Min_Positive", "(MinDaysBetween IS NULL) OR (MinDaysBetween >= 1)");
                    table.CheckConstraint("CK_WG_Month_End", "SummerEndMonth BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_WG_Month_Start", "SummerStartMonth BETWEEN 1 AND 12");
                });

            migrationBuilder.CreateTable(
                name: "Plants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    LatinName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Area = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    PotVolumeMl = table.Column<int>(type: "INTEGER", nullable: true),
                    ImagePath = table.Column<string>(type: "TEXT", maxLength: 400, nullable: true),
                    Mode = table.Column<int>(type: "INTEGER", nullable: false),
                    WateringGroupId = table.Column<int>(type: "INTEGER", nullable: true),
                    SensorEntityId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    MoistureLowPercent = table.Column<double>(type: "REAL", nullable: true),
                    MoistureHighPercent = table.Column<double>(type: "REAL", nullable: true),
                    LastWateredUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plants", x => x.Id);
                    table.CheckConstraint("CK_Plant_MoistureHigh_0_100", "(MoistureHighPercent IS NULL) OR (MoistureHighPercent BETWEEN 0 AND 100)");
                    table.CheckConstraint("CK_Plant_MoistureLow_0_100", "(MoistureLowPercent IS NULL) OR (MoistureLowPercent BETWEEN 0 AND 100)");
                    table.CheckConstraint("CK_Plant_PotVolume_Positive", "(PotVolumeMl IS NULL) OR (PotVolumeMl > 0)");
                    table.CheckConstraint("CK_Plant_ScheduleGroup", "(Mode <> 0) OR (WateringGroupId IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Plants_WateringGroups_WateringGroupId",
                        column: x => x.WateringGroupId,
                        principalTable: "WateringGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Plants_Name",
                table: "Plants",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_WateringGroupId",
                table: "Plants",
                column: "WateringGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_WateringGroups_Name",
                table: "WateringGroups",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Plants");

            migrationBuilder.DropTable(
                name: "WateringGroups");
        }
    }
}
