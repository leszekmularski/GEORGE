using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration49 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataZapisu",
                table: "KonfSystem",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "KtoZapisal",
                table: "KonfSystem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SystemyOkienne",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nazwa_Systemu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Opis_Systemu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Uwagi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Wycofany_z_produkcji = table.Column<bool>(type: "bit", nullable: false),
                    DataZapisu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KtoZapisal = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemyOkienne", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemyOkienne");

            migrationBuilder.DropColumn(
                name: "DataZapisu",
                table: "KonfSystem");

            migrationBuilder.DropColumn(
                name: "KtoZapisal",
                table: "KonfSystem");
        }
    }
}
