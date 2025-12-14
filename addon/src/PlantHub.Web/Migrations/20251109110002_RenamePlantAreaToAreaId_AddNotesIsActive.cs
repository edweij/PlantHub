using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantHub.Web.Migrations
{
    /// <inheritdoc />
    public partial class RenamePlantAreaToAreaId_AddNotesIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Area",
                table: "Plants",
                newName: "AreaId");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Plants",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Plants",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Plants");

            migrationBuilder.RenameColumn(
                name: "AreaId",
                table: "Plants",
                newName: "Area");
        }
    }
}
