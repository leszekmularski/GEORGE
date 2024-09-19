using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration32 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElemetZamDoZlecen",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowIdZlecenia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowIdProducent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Szerokosc = table.Column<float>(type: "real", nullable: true),
                    Wysokosc = table.Column<float>(type: "real", nullable: true),
                    Dlugosc = table.Column<float>(type: "real", nullable: true),
                    Waga = table.Column<float>(type: "real", nullable: true),
                    Powierzchnia = table.Column<float>(type: "real", nullable: true),
                    Objetosc = table.Column<float>(type: "real", nullable: true),
                    CenaNetto = table.Column<float>(type: "real", nullable: true),
                    Typ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumerKatalogowy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NazwaProduktu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Kolor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IloscSztuk = table.Column<float>(type: "real", nullable: false),
                    Uwagi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataZamowienia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataRealizacji = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataZapisu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KtoZapisal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OstatniaZmiana = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CzyZamowiono = table.Column<bool>(type: "bit", nullable: false),
                    PozDostarczono = table.Column<bool>(type: "bit", nullable: false),
                    DataDostarczenia = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElemetZamDoZlecen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProducenciPodwykonawcy",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NazwaProducenta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Adres = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Miejscowosc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    REGON = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdresWWW1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdresWWW2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Uwagi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OsobaKontaktowa1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailOsobyKontaktowej1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TelefonOsobyKontaktowej1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OsobaKontaktowa2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailOsobyKontaktowej2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TelefonOsobyKontaktowej2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OsobaKontaktowa3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailOsobyKontaktowej3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TelefonOsobyKontaktowej3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IloscDniRealizacji = table.Column<int>(type: "int", nullable: false),
                    DataZapisu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KtoZapisal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OstatniaZmiana = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KlienetWymagaProforma = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducenciPodwykonawcy", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElemetZamDoZlecen");

            migrationBuilder.DropTable(
                name: "ProducenciPodwykonawcy");
        }
    }
}
