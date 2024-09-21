using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration34 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NumerZlecenia",
                table: "ZleceniaProdukcyjneWew",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumerZlecenia",
                table: "ZleceniaProdukcyjne",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumerZlecenia",
                table: "ZleceniaProdukcyjneWew");

            migrationBuilder.DropColumn(
                name: "NumerZlecenia",
                table: "ZleceniaProdukcyjne");
        }
    }
}
