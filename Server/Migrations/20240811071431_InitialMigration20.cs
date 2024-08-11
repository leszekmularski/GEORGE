using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration20 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Logowania",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uzytkownik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RodzajPrzegladarki = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Datalogowania = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logowania", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pracownicy",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowIdDzialu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Kodkreskowy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Imie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nazwisko = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Stanowisko = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StanowiskoSystem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dzial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UzytkownikSQL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasloSQL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Uwagi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notatka = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Datautowrzenia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Autorzmiany = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nieaktywny = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pracownicy", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Logowania");

            migrationBuilder.DropTable(
                name: "Pracownicy");
        }
    }
}
