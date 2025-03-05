using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration50 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KonfModele",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowIdSystem = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowIdElement = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Typ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KatWystapieniaZakresOdMin = table.Column<int>(type: "int", nullable: true),
                    KatWystapieniaZakresOdMax = table.Column<int>(type: "int", nullable: true),
                    PromienWystapieniaZakresOdMin = table.Column<int>(type: "int", nullable: true),
                    PromienWystapieniaZakresOdMax = table.Column<int>(type: "int", nullable: true),
                    KonstrMinSzer = table.Column<int>(type: "int", nullable: true),
                    KonstrMaxSzer = table.Column<int>(type: "int", nullable: true),
                    KonstrMinWys = table.Column<int>(type: "int", nullable: true),
                    KonstrMaxWys = table.Column<int>(type: "int", nullable: true),
                    DodatkowyFiltrWystepowania = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Uwagi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rysunek = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    DataZapisu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KtoZapisal = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KonfModele", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KonfModele");
        }
    }
}
