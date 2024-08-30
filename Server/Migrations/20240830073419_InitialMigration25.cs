using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration25 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "JednostkiNaZlecenie",
                table: "ZleceniaProdukcyjneWew",
                type: "real",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataRozpProdukcji",
                table: "ZleceniaProdukcyjneWew",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<float>(
                name: "JednostkiNaZlecenie",
                table: "ZleceniaProdukcyjne",
                type: "real",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataRozpProdukcji",
                table: "ZleceniaProdukcyjne",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "PozDoZlecen",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RwoIdZlecenia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nr = table.Column<float>(type: "real", nullable: false),
                    IloscOkien = table.Column<int>(type: "int", nullable: false),
                    JednostkiOkienDoPoz = table.Column<float>(type: "real", nullable: false),
                    JednostkiOkienSumaDoPoz = table.Column<float>(type: "real", nullable: false),
                    JednostkiOkienDoPozZrobione = table.Column<float>(type: "real", nullable: false),
                    Uwagi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataZapisu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KtoZapisal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OstatniaZmiana = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PozDoZlecen", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PozDoZlecen");

            migrationBuilder.DropColumn(
                name: "DataRozpProdukcji",
                table: "ZleceniaProdukcyjneWew");

            migrationBuilder.DropColumn(
                name: "DataRozpProdukcji",
                table: "ZleceniaProdukcyjne");

            migrationBuilder.AlterColumn<int>(
                name: "JednostkiNaZlecenie",
                table: "ZleceniaProdukcyjneWew",
                type: "int",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<int>(
                name: "JednostkiNaZlecenie",
                table: "ZleceniaProdukcyjne",
                type: "int",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");
        }
    }
}
