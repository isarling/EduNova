namespace EduNova.Models;

public class PoliticaCredito
{
    public int Id { get; set; }
    public decimal DeudaMaxima { get; set; } = 5000m;
    public int MaxCursosSinDeuda { get; set; } = 4;
    public int MaxCursosConDeuda { get; set; } = 2;
    public decimal PorcentajeInicial { get; set; } = 20m;
    public decimal AbonoMinimo { get; set; } = 100m;
    public int MaxCuotas { get; set; } = 6;
    public int DiasEntreCuotas { get; set; } = 30;
    public int DiasGracia { get; set; } = 7;
    public decimal PorcentajeBeneficio { get; set; } = 5m;
    public int DiasVigenciaBeneficio { get; set; } = 60;
}
public class CategoriaCurso
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public ICollection<Curso> Cursos { get; set; } = new List<Curso>();
}
public class ModalidadCurso
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public ICollection<Curso> Cursos { get; set; } = new List<Curso>();
}
public class Cuota
{
    public int Id { get; set; }
    public int DetalleMatriculaId { get; set; }
    public DetalleMatricula DetalleMatricula { get; set; } = null!;
    public int Numero { get; set; }
    public DateTime Vencimiento { get; set; }
    public decimal Monto { get; set; }
    public decimal Pagado { get; set; }
}
public class DetallePago
{
    public int Id { get; set; }
    public int PagoId { get; set; }
    public Pago Pago { get; set; } = null!;
    public int CuotaId { get; set; }
    public Cuota Cuota { get; set; } = null!;
    public decimal Monto { get; set; }
}
public class ReciboPago
{
    public int Id { get; set; }
    public int PagoId { get; set; }
    public Pago Pago { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public string NombreEstudiante { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public decimal SaldoPosterior { get; set; }
    public ICollection<DetalleRecibo> Detalles { get; set; } = new List<DetalleRecibo>();
}
public class DetalleRecibo
{
    public int Id { get; set; }
    public int ReciboPagoId { get; set; }
    public ReciboPago ReciboPago { get; set; } = null!;
    public string Curso { get; set; } = string.Empty;
    public int NumeroCuota { get; set; }
    public DateTime Vencimiento { get; set; }
    public decimal Monto { get; set; }
}
public class BeneficioDescuento
{
    public int Id { get; set; }
    public int EstudianteId { get; set; }
    public Estudiante Estudiante { get; set; } = null!;
    public int MatriculaOrigenId { get; set; }
    public Matricula MatriculaOrigen { get; set; } = null!;
    public int? MatriculaDestinoId { get; set; }
    public Matricula? MatriculaDestino { get; set; }
    public DateTime Emitido { get; set; }
    public DateTime Vence { get; set; }
    public decimal Porcentaje { get; set; }
}
public class ConstanciaSolvencia
{
    public int Id { get; set; }
    public int EstudianteId { get; set; }
    public Estudiante Estudiante { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public string NombreEstudiante { get; set; } = string.Empty;
    public decimal TotalContratado { get; set; }
    public decimal TotalPagado { get; set; }
    public ICollection<DetalleSolvencia> Detalles { get; set; } = new List<DetalleSolvencia>();
}
public class DetalleSolvencia
{
    public int Id { get; set; }
    public int ConstanciaSolvenciaId { get; set; }
    public ConstanciaSolvencia ConstanciaSolvencia { get; set; } = null!;
    public int MatriculaId { get; set; }
    public Matricula Matricula { get; set; } = null!;
    public decimal Total { get; set; }
    public decimal Pagado { get; set; }
}
