using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration31 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataDostarczenia",
                table: "KantowkaDoZlecen",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "PozDostarczono",
                table: "KantowkaDoZlecen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SzybyDoZlecen",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowIdZlecenia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NazwaProduktu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Szerokosc = table.Column<float>(type: "real", nullable: true),
                    Wysokosc = table.Column<float>(type: "real", nullable: true),
                    RodzajSzyby = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RodzajRamki = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IloscSztuk = table.Column<int>(type: "int", nullable: false),
                    Uwagi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataZamowienia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataRealizacji = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataZapisu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KtoZapisal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OstatniaZmiana = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CzyKsztalt = table.Column<bool>(type: "bit", nullable: false),
                    PozDostarczono = table.Column<bool>(type: "bit", nullable: false),
                    DataDostarczenia = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SzybyDoZlecen", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SzybyDoZlecen");

            migrationBuilder.DropColumn(
                name: "DataDostarczenia",
                table: "KantowkaDoZlecen");

            migrationBuilder.DropColumn(
                name: "PozDostarczono",
                table: "KantowkaDoZlecen");
        }
    }
}
