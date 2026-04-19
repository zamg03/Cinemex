using System.ComponentModel.DataAnnotations;

namespace Cinemex.Models
{
    public class Reserva
    {
        [Key]
        public int Id { get; set; }
        public string UsuarioEmail { get; set; }
        public string Horario { get; set; }
        public string Asiento { get; set; }
        public string Cine { get; set; } // Nuevo campo para diferenciar sucursales
    }
}