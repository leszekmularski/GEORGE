using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KantowkaDoZlecen",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowIdZlecenia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GatunekKantowki = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Przekroj = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NazwaProduktu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KodProduktu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Uwagi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DlugoscZamawiana = table.Column<int>(type: "int", nullable: false),
                    DlugoscNaGotowo = table.Column<int>(type: "int", nullable: false),
                    IloscSztuk = table.Column<int>(type: "int", nullable: false),
                    DataZamowienia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataRealizacji = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataZapisu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KtoZapisal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OstatniaZmiana = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KantowkaDoZlecen", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KantowkaDoZlecen");
        }
    }
}
