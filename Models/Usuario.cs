using System.ComponentModel.DataAnnotations;

namespace Cinemex.Models
{
    // Esta clase representa la tabla "Usuarios" en la base de datos SQLite.
    public class Usuario
    {
        // [Key] le dice a la base de datos que este es el identificador único (Primary Key) y se autoincrementará.
        [Key]
        public int Id { get; set; }

        // [Required] hace que el campo sea obligatorio. Si está vacío, la base de datos lo rechaza.
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Nombres { get; set; }

        [Required]
        public string ApellidoPaterno { get; set; }

        // Este campo no tiene [Required], por lo que el usuario puede dejarlo en blanco de forma segura.
        public string ApellidoMaterno { get; set; }

        [Required]
        public string Telefono { get; set; }

        [Required]
        public string FechaNacimiento { get; set; }
    }
}