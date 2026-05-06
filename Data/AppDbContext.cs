using Cinemex.Models;
using Microsoft.EntityFrameworkCore;

namespace Cinemex.Data
{
    // Heredamos de DbContext, que es la clase maestra de Entity Framework Core
    public class AppDbContext : DbContext
    {
        // El constructor recibe las opciones de configuración (ej. que usamos SQLite) desde Program.cs
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet le indica a la base de datos que debe crear una tabla llamada "Usuarios" basada en el modelo Usuario.cs
        public DbSet<Usuario> Usuarios { get; set; }

        // DbSet le indica a la base de datos que debe crear una tabla llamada "Reservas" basada en el modelo Reserva.cs
        public DbSet<Reserva> Reservas { get; set; }
    }
}