using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration26 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Ciezar1Sztuki",
                table: "PozDoZlecen",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "Iloscskrzydel",
                table: "PozDoZlecen",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Kolor",
                table: "PozDoZlecen",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "System",
                table: "PozDoZlecen",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Szyba",
                table: "PozDoZlecen",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Technologia",
                table: "PozDoZlecen",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ciezar1Sztuki",
                table: "PozDoZlecen");

            migrationBuilder.DropColumn(
                name: "Iloscskrzydel",
                table: "PozDoZlecen");

            migrationBuilder.DropColumn(
                name: "Kolor",
                table: "PozDoZlecen");

            migrationBuilder.DropColumn(
                name: "System",
                table: "PozDoZlecen");

            migrationBuilder.DropColumn(
                name: "Szyba",
                table: "PozDoZlecen");

            migrationBuilder.DropColumn(
                name: "Technologia",
                table: "PozDoZlecen");
        }
    }
}
