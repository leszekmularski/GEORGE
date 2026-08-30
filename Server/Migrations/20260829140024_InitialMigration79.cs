using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration79 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Model2PozycjaX",
                table: "KonfPolaczenie",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Model2PozycjaY",
                table: "KonfPolaczenie",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModelsGroupTransform",
                table: "KonfPolaczenie",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ZoomLevel",
                table: "KonfPolaczenie",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZoomTransform",
                table: "KonfPolaczenie",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Model2PozycjaX",
                table: "KonfPolaczenie");

            migrationBuilder.DropColumn(
                name: "Model2PozycjaY",
                table: "KonfPolaczenie");

            migrationBuilder.DropColumn(
                name: "ModelsGroupTransform",
                table: "KonfPolaczenie");

            migrationBuilder.DropColumn(
                name: "ZoomLevel",
                table: "KonfPolaczenie");

            migrationBuilder.DropColumn(
                name: "ZoomTransform",
                table: "KonfPolaczenie");
        }
    }
}
