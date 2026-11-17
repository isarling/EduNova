using System.ComponentModel.DataAnnotations;
using EduNova.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace EduNova.DTOS;
public class CrearMatriculaDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un estudiante.")]
    public int EstudianteId { get; set; }
    [Range(typeof(decimal), "0", "99999999.99")] public decimal PagoInicial { get; set; }
    public bool AplicarBeneficio { get; set; }
    public Guid OperacionId { get; set; } = Guid.NewGuid();
    [ValidateNever] public PoliticaCredito Politica { get; set; } = new();
    public List<LineaMatriculaDto> Lineas { get; set; } = new();
    [ValidateNever] public List<OpcionAcademiaDto> Estudiantes { get; set; } = new();
}
public class LineaMatriculaDto
{
    public bool Seleccionado { get; set; }
    public int CursoId { get; set; }
    [Range(1, 24)] public int CantidadCuotas { get; set; } = 1;
    [ValidateNever] public string Nombre { get; set; } = string.Empty;
    [BindNever, ValidateNever] public decimal Costo { get; set; }
}
public class OpcionAcademiaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
public class RegistrarPagoDto
{
    [Range(1, int.MaxValue)] public int MatriculaId { get; set; }
    [Range(typeof(decimal), "0.01", "99999999.99", ErrorMessage = "Ingresa un monto positivo.")]
    public decimal Monto { get; set; }
    [StringLength(100)] public string? Referencia { get; set; }
    public Guid OperacionId { get; set; }
}
public class MatriculaResumenDto
{
    public int Id { get; set; }
    public string Estudiante { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public decimal Pagado { get; set; }
    public decimal Saldo => Total - Pagado;
}
public class MatriculaDetalleDto
{
    public MatriculaResumenDto Resumen { get; set; } = new();
    public List<LineaMatriculaDto> Cursos { get; set; } = new();
    public List<PagoResumenDto> Pagos { get; set; } = new();
    public List<CuotaDto> Cuotas { get; set; } = new();
    public int EstudianteId { get; set; }
    public RegistrarPagoDto NuevoPago { get; set; } = new();
}
public class PagoResumenDto
{
    public int? ReciboId { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }
    public string Referencia { get; set; } = string.Empty;
}

public class ConsultaMatriculasDto
{
    public string Busqueda { get; set; } = string.Empty;

    public List<MatriculaResumenDto> Matriculas { get; set; } = new();

    public List<ConsultaPersonaDto> Personas { get; set; } = new();
}

public class ConsultaPersonaDto
{
    public int EstudianteId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int CantidadCursos { get; set; }

    public decimal Total { get; set; }

    public decimal Pagado { get; set; }

    public decimal Saldo => Total - Pagado;

    public List<CursoInscritoDto> Cursos { get; set; } = new();

    public List<CuotaDto> Cuotas { get; set; } = new();

    public List<PagoResumenDto> Pagos { get; set; } = new();
}

public class CursoInscritoDto
{
    public int MatriculaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }

    public decimal Costo { get; set; }

    public decimal Pagado { get; set; }

    public decimal Saldo => Costo - Pagado;
}