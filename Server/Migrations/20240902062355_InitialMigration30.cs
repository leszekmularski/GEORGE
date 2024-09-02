using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration30 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProcentWykonania",
                table: "ZleceniaProdukcyjneWew",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProcentWykonania",
                table: "ZleceniaProdukcyjne",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcentWykonania",
                table: "ZleceniaProdukcyjneWew");

            migrationBuilder.DropColumn(
                name: "ProcentWykonania",
                table: "ZleceniaProdukcyjne");
        }
    }
}
