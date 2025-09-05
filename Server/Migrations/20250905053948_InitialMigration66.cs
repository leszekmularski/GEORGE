using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration66 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowIdElement",
                table: "KonfModele");

            migrationBuilder.CreateTable(
                name: "WzorceKompltacji",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowIdProducent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Szerokosc = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Wysokosc = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Dlugosc = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Waga = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Powierzchnia = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Objetosc = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CenaNetto = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Jednostka = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Typ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumerKatalogowy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NazwaProduktu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Kolor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ilosc = table.Column<double>(type: "float", nullable: false),
                    CzasRealizacjiZamowienia = table.Column<double>(type: "float", nullable: false),
                    Uwagi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataZapisu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KtoZapisal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OstatniaZmiana = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowIdPliku_1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowIdPliku_2 = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WzorceKompltacji", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WzorceKompltacji");

            migrationBuilder.AddColumn<Guid>(
                name: "RowIdElement",
                table: "KonfModele",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
