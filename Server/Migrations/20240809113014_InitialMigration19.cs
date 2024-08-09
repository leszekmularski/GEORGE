using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration19 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ZleceniaCzasNaLinieProd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowIdLinieProdukcyjne = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowIdZleceniaProdukcyjne = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CzasNaZlecenie = table.Column<int>(type: "int", nullable: false),
                    ZlecenieWewnetrzne = table.Column<bool>(type: "bit", nullable: false),
                    Uwagi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataZapisu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KtoZapisal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OstatniaZmiana = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZleceniaCzasNaLinieProd", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ZleceniaCzasNaLinieProd");
        }
    }
}
