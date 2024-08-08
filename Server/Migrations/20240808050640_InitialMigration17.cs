using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration17 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ZlecZrealizowane",
                table: "ZleceniaProdukcyjneWew",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ZlecZrealizowane",
                table: "ZleceniaProdukcyjne",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ZlecZrealizowane",
                table: "ZleceniaProdukcyjneWew");

            migrationBuilder.DropColumn(
                name: "ZlecZrealizowane",
                table: "ZleceniaProdukcyjne");
        }
    }
}
