using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration15 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NumerKarty",
                table: "LinieProdukcyjne",
                newName: "IdLiniiProdukcyjnej");

            migrationBuilder.AlterColumn<string>(
                name: "RowIdZleceniaProdukcyjne",
                table: "ZleceniaNaLinii",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdLiniiProdukcyjnej",
                table: "LinieProdukcyjne",
                newName: "NumerKarty");

            migrationBuilder.AlterColumn<int>(
                name: "RowIdZleceniaProdukcyjne",
                table: "ZleceniaNaLinii",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
