using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinemex.Migrations
{
    /// <inheritdoc />
    public partial class AgregandoPelicula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Pelicula",
                table: "Reservas",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pelicula",
                table: "Reservas");
        }
    }
}
