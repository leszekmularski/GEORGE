using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration47 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KonfSystem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowIdSystem = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PionLewa = table.Column<int>(type: "int", nullable: true),
                    PionPrawa = table.Column<int>(type: "int", nullable: true),
                    PionOdSzybaOdZew = table.Column<int>(type: "int", nullable: true),
                    PionDodatkowa4 = table.Column<int>(type: "int", nullable: true),
                    PionDodatkowa5 = table.Column<int>(type: "int", nullable: true),
                    PoziomDol = table.Column<int>(type: "int", nullable: true),
                    PoziomGora = table.Column<int>(type: "int", nullable: true),
                    PoziomKorpus = table.Column<int>(type: "int", nullable: true),
                    PoziomLiniaSzkla = table.Column<int>(type: "int", nullable: true),
                    PoziomLiniaOkucia = table.Column<int>(type: "int", nullable: true),
                    PoziomOsDormas = table.Column<int>(type: "int", nullable: true),
                    PoziomDodatkowa6 = table.Column<int>(type: "int", nullable: true),
                    PoziomDodatkowa7 = table.Column<int>(type: "int", nullable: true),
                    Indeks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nazwa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Typ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WystepujeDol = table.Column<bool>(type: "bit", nullable: false),
                    WystepujeLewa = table.Column<bool>(type: "bit", nullable: false),
                    WystepujeGora = table.Column<bool>(type: "bit", nullable: false),
                    WystepujePrawa = table.Column<bool>(type: "bit", nullable: false),
                    KatWystapieniaZakresOdMin = table.Column<int>(type: "int", nullable: true),
                    KatWystapieniaZakresOdMax = table.Column<int>(type: "int", nullable: true),
                    ZakresStosDlugoscOdMin = table.Column<int>(type: "int", nullable: true),
                    ZakresStosDlugoscOdMax = table.Column<int>(type: "int", nullable: true),
                    DodatkowyFiltrWystepowania = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Uwagi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SVG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cena1MB = table.Column<double>(type: "float", nullable: true),
                    Waga = table.Column<double>(type: "float", nullable: true),
                    WymiarXKantowki1 = table.Column<int>(type: "int", nullable: true),
                    WymiarYKantowki1 = table.Column<int>(type: "int", nullable: true),
                    Cena1MBKantowki1 = table.Column<double>(type: "float", nullable: true),
                    WagaKantowki1 = table.Column<double>(type: "float", nullable: true),
                    DlugoscKantowki1 = table.Column<double>(type: "float", nullable: true),
                    IloscSztukKantowki1 = table.Column<double>(type: "float", nullable: true),
                    WymiarXKantowki2 = table.Column<int>(type: "int", nullable: true),
                    WymiarYKantowki2 = table.Column<int>(type: "int", nullable: true),
                    Cena1MBKantowki2 = table.Column<double>(type: "float", nullable: true),
                    WagaKantowki2 = table.Column<double>(type: "float", nullable: true),
                    DlugoscKantowki2 = table.Column<double>(type: "float", nullable: true),
                    IloscSztukKantowki2 = table.Column<double>(type: "float", nullable: true),
                    WymiarXKantowki3 = table.Column<int>(type: "int", nullable: true),
                    WymiarYKantowki3 = table.Column<int>(type: "int", nullable: true),
                    Cena1MBKantowki3 = table.Column<double>(type: "float", nullable: true),
                    WagaKantowki3 = table.Column<double>(type: "float", nullable: true),
                    DlugoscKantowki3 = table.Column<double>(type: "float", nullable: true),
                    IloscSztukKantowki3 = table.Column<double>(type: "float", nullable: true),
                    WymiarXKantowki4 = table.Column<int>(type: "int", nullable: true),
                    WymiarYKantowki4 = table.Column<int>(type: "int", nullable: true),
                    Cena1MBKantowki4 = table.Column<double>(type: "float", nullable: true),
                    WagaKantowki4 = table.Column<double>(type: "float", nullable: true),
                    DlugoscKantowki4 = table.Column<double>(type: "float", nullable: true),
                    IloscSztukKantowki4 = table.Column<double>(type: "float", nullable: true),
                    WymiarXKantowki5 = table.Column<int>(type: "int", nullable: true),
                    WymiarYKantowki5 = table.Column<int>(type: "int", nullable: true),
                    Cena1MBKantowki5 = table.Column<double>(type: "float", nullable: true),
                    WagaKantowki5 = table.Column<double>(type: "float", nullable: true),
                    DlugoscKantowki5 = table.Column<double>(type: "float", nullable: true),
                    IloscSztukKantowki5 = table.Column<double>(type: "float", nullable: true),
                    Rysunek = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KonfSystem", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KonfSystem");
        }
    }
}
