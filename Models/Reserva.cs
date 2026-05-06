using System.ComponentModel.DataAnnotations;

namespace Cinemex.Models
{
    // Esta clase representa la tabla "Reservas" para mantener un historial de asientos ocupados.
    public class Reserva
    {
        [Key]
        public int Id { get; set; }

        // Relacionamos la compra con el correo del cliente que inició sesión.
        public string UsuarioEmail { get; set; }

        // Datos específicos de la función para evitar que los asientos se empalmen en otros días o salas.
        public string Pelicula { get; set; }
        public string Fecha { get; set; }
        public string Sala { get; set; }
        public string Horario { get; set; }

        // Guardamos el código exacto de la butaca (Ej. "G5")
        public string Asiento { get; set; }
        public string Cine { get; set; }
    }
}