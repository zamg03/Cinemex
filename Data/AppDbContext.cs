using Microsoft.EntityFrameworkCore;
using Cinemex.Models;

namespace Cinemex.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Reserva> Reservas { get; set; } // Nueva tabla
    }
}