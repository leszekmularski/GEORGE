using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEORGE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration82 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ElementWewnetrznyToSlupek",
                table: "KonfPolaczenie",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ElementZewnetrznyToSlupek",
                table: "KonfPolaczenie",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ElementWewnetrznyToSlupek",
                table: "KonfPolaczenie");

            migrationBuilder.DropColumn(
                name: "ElementZewnetrznyToSlupek",
                table: "KonfPolaczenie");
        }
    }
}
