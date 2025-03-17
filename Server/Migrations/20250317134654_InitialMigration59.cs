using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration59 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "WidocznaNaLiscie",
                table: "KonfModele",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WidocznaNaLiscie",
                table: "KonfModele");
        }
    }
}
