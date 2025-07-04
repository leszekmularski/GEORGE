using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration62 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KonfPolaczenie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ElementZewnetrznyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ElementZewnetrznyId1 = table.Column<int>(type: "int", nullable: true),
                    ElementWewnetrznyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ElementWewnetrznyId1 = table.Column<int>(type: "int", nullable: true),
                    StronaPolaczenia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Luz = table.Column<double>(type: "float", nullable: false),
                    KatOd = table.Column<int>(type: "int", nullable: true),
                    KatDo = table.Column<int>(type: "int", nullable: true),
                    DodatkowyWarunek = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Uwagi = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KonfPolaczenie", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KonfPolaczenie_KonfSystem_ElementWewnetrznyId1",
                        column: x => x.ElementWewnetrznyId1,
                        principalTable: "KonfSystem",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_KonfPolaczenie_KonfSystem_ElementZewnetrznyId1",
                        column: x => x.ElementZewnetrznyId1,
                        principalTable: "KonfSystem",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_KonfPolaczenie_ElementWewnetrznyId1",
                table: "KonfPolaczenie",
                column: "ElementWewnetrznyId1");

            migrationBuilder.CreateIndex(
                name: "IX_KonfPolaczenie_ElementZewnetrznyId1",
                table: "KonfPolaczenie",
                column: "ElementZewnetrznyId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KonfPolaczenie");
        }
    }
}
