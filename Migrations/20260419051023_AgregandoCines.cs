using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinemex.Migrations
{
    /// <inheritdoc />
    public partial class AgregandoCines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cine",
                table: "Reservas",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cine",
                table: "Reservas");
        }
    }
}
