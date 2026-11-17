namespace EduNova.Models;
public class Matricula
{
    public int Id { get; set; }
    public int EstudianteId { get; set; }
    public Estudiante Estudiante { get; set; } = null!;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public Guid? OperacionId { get; set; }
    public decimal MinimoInicialAplicado { get; set; }
    public ICollection<DetalleMatricula> Detalles { get; set; } = new List<DetalleMatricula>();
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
public class DetalleMatricula
{
    public int Id { get; set; }
    public int MatriculaId { get; set; }
    public Matricula Matricula { get; set; } = null!;
    public int CursoId { get; set; }
    public Curso Curso { get; set; } = null!;
    public decimal Costo { get; set; }
    public decimal PrecioCatalogo { get; set; }
    public decimal Descuento { get; set; }
    public DateTime FechaFin { get; set; }
    public ICollection<Cuota> Cuotas { get; set; } = new List<Cuota>();
}
public class Pago
{
    public int Id { get; set; }
    public int MatriculaId { get; set; }
    public Matricula Matricula { get; set; } = null!;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public decimal Monto { get; set; }
    public string Referencia { get; set; } = string.Empty;
    public Guid OperacionId { get; set; }
    public ICollection<DetallePago> Detalles { get; set; } = new List<DetallePago>();
    public ReciboPago? Recibo { get; set; }
}
