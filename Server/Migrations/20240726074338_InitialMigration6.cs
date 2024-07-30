using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NumerRodzajuKart",
                table: "RodzajeKartInstrukcyjnych",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OryginalnaNazwaPliku",
                table: "PlikiZlecenProdukcyjnych",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RodzajeKartInstrukcyjnych_NumerRodzajuKart",
                table: "RodzajeKartInstrukcyjnych",
                column: "NumerRodzajuKart",
                unique: true,
                filter: "[NumerRodzajuKart] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RodzajeKartInstrukcyjnych_NumerRodzajuKart",
                table: "RodzajeKartInstrukcyjnych");

            migrationBuilder.DropColumn(
                name: "OryginalnaNazwaPliku",
                table: "PlikiZlecenProdukcyjnych");

            migrationBuilder.AlterColumn<string>(
                name: "NumerRodzajuKart",
                table: "RodzajeKartInstrukcyjnych",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
