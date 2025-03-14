using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration57 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "PionOsSymetrii",
                table: "KonfSystem",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PoziomOsSymetrii",
                table: "KonfSystem",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PionOsSymetrii",
                table: "KonfSystem");

            migrationBuilder.DropColumn(
                name: "PoziomOsSymetrii",
                table: "KonfSystem");
        }
    }
}
