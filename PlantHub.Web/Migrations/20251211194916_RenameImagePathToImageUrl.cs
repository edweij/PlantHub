using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantHub.Web.Migrations
{
    /// <inheritdoc />
    public partial class RenameImagePathToImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "Plants",
                newName: "ImageUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Plants",
                newName: "ImagePath");
        }        
    }
}
