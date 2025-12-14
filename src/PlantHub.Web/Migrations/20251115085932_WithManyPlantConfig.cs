using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantHub.Web.Migrations
{
    /// <inheritdoc />
    public partial class WithManyPlantConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plants_WateringGroups_WateringGroupId1",
                table: "Plants");

            migrationBuilder.DropIndex(
                name: "IX_Plants_WateringGroupId1",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "WateringGroupId1",
                table: "Plants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WateringGroupId1",
                table: "Plants",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plants_WateringGroupId1",
                table: "Plants",
                column: "WateringGroupId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Plants_WateringGroups_WateringGroupId1",
                table: "Plants",
                column: "WateringGroupId1",
                principalTable: "WateringGroups",
                principalColumn: "Id");
        }
    }
}
