
namespace EduNova.Models
{
    public class Estudiante
    {
        public int Id { get; set; }
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public string? Nombre { get; set; }

        public string? Apellido { get; set; }

        public string? Email { get; set; }
        // Compatibilidad con registros anteriores. Las inscripciones se gestionan con Matricula.
        public int? CursoId { get; set; }
        public virtual Curso? Curso { get; set; }
    }
}
