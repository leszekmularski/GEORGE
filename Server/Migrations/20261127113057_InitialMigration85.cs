using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration85 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PakietySzybowe",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nazwa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Aktywny = table.Column<bool>(type: "bit", nullable: false),
                    Ug = table.Column<double>(type: "float", nullable: false),
                    Lt = table.Column<double>(type: "float", nullable: false),
                    g = table.Column<double>(type: "float", nullable: false),
                    Rw = table.Column<double>(type: "float", nullable: false),
                    CenaZaM2 = table.Column<double>(type: "float", nullable: false),
                    DataUtworzenia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataModyfikacji = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PakietySzybowe", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WarstwySzyby",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PakietSzybowyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kolejnosc = table.Column<int>(type: "int", nullable: false),
                    Typ = table.Column<int>(type: "int", nullable: false),
                    Grubosc = table.Column<double>(type: "float", nullable: false),
                    RodzajSzyby = table.Column<int>(type: "int", nullable: false),
                    Powloka = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Kolor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RodzajRamki = table.Column<int>(type: "int", nullable: false),
                    Gaz = table.Column<int>(type: "int", nullable: false),
                    Producent = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Uwagi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarstwySzyby", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarstwySzyby_PakietySzybowe_PakietSzybowyId",
                        column: x => x.PakietSzybowyId,
                        principalTable: "PakietySzybowe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PakietySzybowe_Nazwa",
                table: "PakietySzybowe",
                column: "Nazwa");

            migrationBuilder.CreateIndex(
                name: "IX_WarstwySzyby_PakietSzybowyId",
                table: "WarstwySzyby",
                column: "PakietSzybowyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WarstwySzyby");

            migrationBuilder.DropTable(
                name: "PakietySzybowe");
        }
    }
}
