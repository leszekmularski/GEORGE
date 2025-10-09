using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration71 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "PrzesuniecieXStycznej",
                table: "KonfPolaczenie",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PrzesuniecieYStycznej",
                table: "KonfPolaczenie",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<byte[]>(
                name: "RysunekPrzekrojuStyczny",
                table: "KonfPolaczenie",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrzesuniecieXStycznej",
                table: "KonfPolaczenie");

            migrationBuilder.DropColumn(
                name: "PrzesuniecieYStycznej",
                table: "KonfPolaczenie");

            migrationBuilder.DropColumn(
                name: "RysunekPrzekrojuStyczny",
                table: "KonfPolaczenie");
        }
    }
}
