using System.ComponentModel.DataAnnotations;

namespace Cinemex.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        // Nuevos campos obligatorios
        [Required]
        public string Nombres { get; set; }

        [Required]
        public string ApellidoPaterno { get; set; }

        public string ApellidoMaterno { get; set; } // Lo dejamos opcional

        [Required]
        public string Telefono { get; set; }

        [Required]
        public string FechaNacimiento { get; set; }
    }
}