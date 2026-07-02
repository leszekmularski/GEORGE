using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration75 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Schody",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Typ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Uwagi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Wysokosc = table.Column<double>(type: "float", nullable: false),
                    Szerokosc = table.Column<double>(type: "float", nullable: false),
                    Glebokosc = table.Column<double>(type: "float", nullable: false),
                    GlebokoscZabieg1 = table.Column<double>(type: "float", nullable: false),
                    GlebokoscZabieg2 = table.Column<double>(type: "float", nullable: false),
                    SzerokoscZabieg1 = table.Column<double>(type: "float", nullable: false),
                    SzerokoscZabieg2 = table.Column<double>(type: "float", nullable: false),
                    RysunekPogladowy = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    RowIdPliku = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Wycofany_z_produkcji = table.Column<bool>(type: "bit", nullable: false),
                    DataZapisu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KtoZapisal = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schody", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Schody");
        }
    }
}
