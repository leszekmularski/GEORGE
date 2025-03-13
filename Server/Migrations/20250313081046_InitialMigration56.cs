using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration56 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SposobLaczeniaCzop",
                table: "KonfModele",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SposobLaczeniaCzop",
                table: "KonfModele");
        }
    }
}
