using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration37 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "WyslanoDoZamowien",
                table: "KantowkaDoZlecen",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WyslanoDoZamowien",
                table: "KantowkaDoZlecen");
        }
    }
}
