using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration36 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailOsobyKontaktowej4",
                table: "ProducenciPodwykonawcy",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailOsobyKontaktowej5",
                table: "ProducenciPodwykonawcy",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OsobaKontaktowa4",
                table: "ProducenciPodwykonawcy",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OsobaKontaktowa5",
                table: "ProducenciPodwykonawcy",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelefonOsobyKontaktowej4",
                table: "ProducenciPodwykonawcy",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelefonOsobyKontaktowej5",
                table: "ProducenciPodwykonawcy",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailOsobyKontaktowej4",
                table: "ProducenciPodwykonawcy");

            migrationBuilder.DropColumn(
                name: "EmailOsobyKontaktowej5",
                table: "ProducenciPodwykonawcy");

            migrationBuilder.DropColumn(
                name: "OsobaKontaktowa4",
                table: "ProducenciPodwykonawcy");

            migrationBuilder.DropColumn(
                name: "OsobaKontaktowa5",
                table: "ProducenciPodwykonawcy");

            migrationBuilder.DropColumn(
                name: "TelefonOsobyKontaktowej4",
                table: "ProducenciPodwykonawcy");

            migrationBuilder.DropColumn(
                name: "TelefonOsobyKontaktowej5",
                table: "ProducenciPodwykonawcy");
        }
    }
}
