using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration63 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KonfPolaczenie_KonfSystem_ElementWewnetrznyId1",
                table: "KonfPolaczenie");

            migrationBuilder.DropForeignKey(
                name: "FK_KonfPolaczenie_KonfSystem_ElementZewnetrznyId1",
                table: "KonfPolaczenie");

            migrationBuilder.DropIndex(
                name: "IX_KonfPolaczenie_ElementWewnetrznyId1",
                table: "KonfPolaczenie");

            migrationBuilder.DropIndex(
                name: "IX_KonfPolaczenie_ElementZewnetrznyId1",
                table: "KonfPolaczenie");

            migrationBuilder.DropColumn(
                name: "ElementWewnetrznyId1",
                table: "KonfPolaczenie");

            migrationBuilder.DropColumn(
                name: "ElementZewnetrznyId1",
                table: "KonfPolaczenie");

            migrationBuilder.RenameColumn(
                name: "Luz",
                table: "KonfPolaczenie",
                newName: "PrzesuniecieY");

            migrationBuilder.AddColumn<double>(
                name: "PrzesuniecieX",
                table: "KonfPolaczenie",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Guid>(
                name: "RowIdSystem",
                table: "KonfPolaczenie",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "ZapisanyKat",
                table: "KonfPolaczenie",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrzesuniecieX",
                table: "KonfPolaczenie");

            migrationBuilder.DropColumn(
                name: "RowIdSystem",
                table: "KonfPolaczenie");

            migrationBuilder.DropColumn(
                name: "ZapisanyKat",
                table: "KonfPolaczenie");

            migrationBuilder.RenameColumn(
                name: "PrzesuniecieY",
                table: "KonfPolaczenie",
                newName: "Luz");

            migrationBuilder.AddColumn<int>(
                name: "ElementWewnetrznyId1",
                table: "KonfPolaczenie",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ElementZewnetrznyId1",
                table: "KonfPolaczenie",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KonfPolaczenie_ElementWewnetrznyId1",
                table: "KonfPolaczenie",
                column: "ElementWewnetrznyId1");

            migrationBuilder.CreateIndex(
                name: "IX_KonfPolaczenie_ElementZewnetrznyId1",
                table: "KonfPolaczenie",
                column: "ElementZewnetrznyId1");

            migrationBuilder.AddForeignKey(
                name: "FK_KonfPolaczenie_KonfSystem_ElementWewnetrznyId1",
                table: "KonfPolaczenie",
                column: "ElementWewnetrznyId1",
                principalTable: "KonfSystem",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_KonfPolaczenie_KonfSystem_ElementZewnetrznyId1",
                table: "KonfPolaczenie",
                column: "ElementZewnetrznyId1",
                principalTable: "KonfSystem",
                principalColumn: "Id");
        }
    }
}
