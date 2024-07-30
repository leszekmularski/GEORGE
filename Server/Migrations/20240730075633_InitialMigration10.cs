using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MaterialZeStanMagazyn",
                table: "KantowkaDoZlecen",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaterialZeStanMagazyn",
                table: "KantowkaDoZlecen");
        }
    }
}
