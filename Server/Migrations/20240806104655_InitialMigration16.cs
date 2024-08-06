using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration16 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JednostkiNaZlecenie",
                table: "ZleceniaProdukcyjneWew",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "JednostkiNaZlecenie",
                table: "ZleceniaProdukcyjne",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JednostkiNaZlecenie",
                table: "ZleceniaProdukcyjneWew");

            migrationBuilder.DropColumn(
                name: "JednostkiNaZlecenie",
                table: "ZleceniaProdukcyjne");
        }
    }
}
