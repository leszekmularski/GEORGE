using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration22 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Uprawnieniapracownika",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowIdPracownicy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowIdRejestrejestrow = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Odczyt = table.Column<bool>(type: "bit", nullable: false),
                    Zapis = table.Column<bool>(type: "bit", nullable: false),
                    Zmiana = table.Column<bool>(type: "bit", nullable: false),
                    Usuniecie = table.Column<bool>(type: "bit", nullable: false),
                    Administrator = table.Column<bool>(type: "bit", nullable: false),
                    Uwagi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Datautowrzenia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Autorzmiany = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uprawnieniapracownika", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Uprawnieniapracownika");
        }
    }
}
