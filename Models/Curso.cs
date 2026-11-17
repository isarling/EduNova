namespace EduNova.Models
{
    public class Curso
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public int DuracionDias { get; set; } = 90;
        public int? CategoriaCursoId { get; set; }
        public CategoriaCurso? CategoriaCurso { get; set; }
        public int? ModalidadCursoId { get; set; }
        public ModalidadCurso? ModalidadCurso { get; set; }
        public decimal Precio { get; set; }

        public ICollection<DetalleMatricula> DetallesMatricula { get; set; } = [];

        public ICollection<Estudiante> Estudiantes { get; set; } = [];
    }

}