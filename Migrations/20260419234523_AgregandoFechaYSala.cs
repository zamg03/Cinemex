using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinemex.Migrations
{
    /// <inheritdoc />
    public partial class AgregandoFechaYSala : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Fecha",
                table: "Reservas",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sala",
                table: "Reservas",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fecha",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "Sala",
                table: "Reservas");
        }
    }
}
