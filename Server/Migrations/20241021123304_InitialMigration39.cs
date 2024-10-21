using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration39 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RowIdPliku",
                table: "SzybyDoZlecen",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RowIdPliku",
                table: "KantowkaDoZlecen",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RowIdPliku",
                table: "ElemetZamDoZlecen",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowIdPliku",
                table: "SzybyDoZlecen");

            migrationBuilder.DropColumn(
                name: "RowIdPliku",
                table: "KantowkaDoZlecen");

            migrationBuilder.DropColumn(
                name: "RowIdPliku",
                table: "ElemetZamDoZlecen");
        }
    }
}
