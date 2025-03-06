using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration54 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NaddatekNaZgrzewNaStrone",
                table: "KonfModele",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ZwiekszNaddatekGdyKatInny90",
                table: "KonfModele",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NaddatekNaZgrzewNaStrone",
                table: "KonfModele");

            migrationBuilder.DropColumn(
                name: "ZwiekszNaddatekGdyKatInny90",
                table: "KonfModele");
        }
    }
}
