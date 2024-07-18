using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataMontazu",
                table: "ZleceniaProdukcyjne",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DataWysylki",
                table: "ZleceniaProdukcyjne",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "RowId",
                table: "ZleceniaProdukcyjne",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlikiZlecenProdukcyjnych",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowIdZleceniaProdukcyjne = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NazwaPliku = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypPliku = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataZapisu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KtoZapisal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OstatniaZmiana = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlikiZlecenProdukcyjnych", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlikiZlecenProdukcyjnych");

            migrationBuilder.DropColumn(
                name: "DataMontazu",
                table: "ZleceniaProdukcyjne");

            migrationBuilder.DropColumn(
                name: "DataWysylki",
                table: "ZleceniaProdukcyjne");

            migrationBuilder.DropColumn(
                name: "RowId",
                table: "ZleceniaProdukcyjne");
        }
    }
}
