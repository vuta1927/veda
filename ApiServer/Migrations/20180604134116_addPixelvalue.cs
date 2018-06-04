using Microsoft.EntityFrameworkCore.Migrations;

namespace ApiServer.Migrations
{
    public partial class addPixelvalue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "height",
                table: "Tags",
                newName: "Height");

            migrationBuilder.AddColumn<double>(
                name: "HeightPixel",
                table: "Tags",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "WidthPixel",
                table: "Tags",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeightPixel",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "WidthPixel",
                table: "Tags");

            migrationBuilder.RenameColumn(
                name: "Height",
                table: "Tags",
                newName: "height");
        }
    }
}
