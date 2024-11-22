﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration42 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TypZamowienia",
                table: "ZleceniaProdukcyjneWew",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RowId",
                table: "ZleceniaProdukcyjneWew",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TypZamowienia",
                table: "ZleceniaProdukcyjne",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RowId",
                table: "ZleceniaProdukcyjne",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ZleceniaProdukcyjneZmianyStatusu",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowIdZlecenia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypZamowienia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumerZamowienia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumerUmowy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataProdukcji = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataWysylki = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataMontazu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Klient = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Adres = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Miejscowosc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NazwaProduktu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NazwaProduktu2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KodProduktu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ilosc = table.Column<int>(type: "int", nullable: false),
                    Wartosc = table.Column<float>(type: "real", nullable: false),
                    DataZapisu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataGotowosci = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KtoZapisal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OstatniaZmiana = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JednostkiNaZlecenie = table.Column<float>(type: "real", nullable: false),
                    ZlecenieWewnatrzne = table.Column<bool>(type: "bit", nullable: false),
                    DataRozpProdukcji = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumerZlecenia = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZleceniaProdukcyjneZmianyStatusu", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ZleceniaProdukcyjneZmianyStatusu");

            migrationBuilder.AlterColumn<string>(
                name: "TypZamowienia",
                table: "ZleceniaProdukcyjneWew",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "RowId",
                table: "ZleceniaProdukcyjneWew",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TypZamowienia",
                table: "ZleceniaProdukcyjne",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "RowId",
                table: "ZleceniaProdukcyjne",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
