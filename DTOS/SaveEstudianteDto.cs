using System.ComponentModel.DataAnnotations;

namespace EduNova.DTOS
{
    public class SaveEstudianteDto
    {
        public int? Id { get; set; }
        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;
        [Required]
        [MaxLength(150)]
        public string Apellido { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
