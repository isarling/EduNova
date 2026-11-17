using System.ComponentModel.DataAnnotations;

namespace EduNova.DTOS
{
    public class SaveCursoDto
    {
        public int? Id { get; set; }
        [Range(1, 730)] public int DuracionDias { get; set; } = 90;
        public int? CategoriaCursoId { get; set; }
        public int? ModalidadCursoId { get; set; }
        [Range(typeof(decimal), "0.01", "99999999.99", ErrorMessage = "El precio debe ser positivo y no superar C$ 99,999,999.99.")]
        public decimal Precio { get; set; }
        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;
        [MaxLength(250)]
        public string? Descripcion { get; set; }

    }
}
