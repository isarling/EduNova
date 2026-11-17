using System.ComponentModel.DataAnnotations;
using EduNova.Models;
namespace EduNova.DTOS;
public class PoliticaCreditoDto
{
    [Range(typeof(decimal), "0", "99999999.99")] public decimal DeudaMaxima { get; set; }
    [Range(1, 20)] public int MaxCursosSinDeuda { get; set; }
    [Range(1, 20)] public int MaxCursosConDeuda { get; set; }
    [Range(typeof(decimal), "0", "100")] public decimal PorcentajeInicial { get; set; }
    [Range(typeof(decimal), "0.01", "99999999.99")] public decimal AbonoMinimo { get; set; }
    [Range(1, 24)] public int MaxCuotas { get; set; }
    [Range(1, 90)] public int DiasEntreCuotas { get; set; }
    [Range(0, 90)] public int DiasGracia { get; set; }
    [Range(typeof(decimal), "0", "50")] public decimal PorcentajeBeneficio { get; set; }
    [Range(1, 365)] public int DiasVigenciaBeneficio { get; set; }
}
public class EstadoEstudianteDto
{
    public int EstudianteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Deuda { get; set; }
    public int CursosActivos { get; set; }
    public bool TieneAtraso { get; set; }
    public string Observacion { get; set; } = string.Empty;
    public List<MatriculaResumenDto> Matriculas { get; set; } = new();
    public List<BeneficioDto> Beneficios { get; set; } = new();
    public List<CuotaDto> Cuotas { get; set; } = new();
    public List<OpcionAcademiaDto> Recibos { get; set; } = new();
    public List<OpcionAcademiaDto> Constancias { get; set; } = new();
}
public class CuotaDto
{
    public int MatriculaId { get; set; }
    public string Curso { get; set; } = string.Empty;
    public int Numero { get; set; }
    public DateTime Vencimiento { get; set; }
    public decimal Monto { get; set; }
    public decimal Pagado { get; set; }
    public decimal Saldo => Monto - Pagado;
}
public class BeneficioDto
{
    public decimal Porcentaje { get; set; }
    public DateTime Vence { get; set; }
    public bool Utilizado { get; set; }
}
public class VincularCuentaDto
{
    public int EstudianteId { get; set; }
    public int? UsuarioId { get; set; }
}
public class VincularVistaDto
{
    public List<OpcionAcademiaDto> Estudiantes { get; set; } = new();
    public List<OpcionAcademiaDto> Usuarios { get; set; } = new();
    public Dictionary<int, int?> Vinculos { get; set; } = new();
}
public class CatalogoDto
{
    [Range(0, int.MaxValue)] public int Id { get; set; }
    [Required, StringLength(80)] public string Nombre { get; set; } = string.Empty;
    [Required] public string Tipo { get; set; } = "categoria";
}
public class CatalogosVistaDto
{
    public List<OpcionAcademiaDto> Categorias { get; set; } = new();
    public List<OpcionAcademiaDto> Modalidades { get; set; } = new();
}
public record ResultadoOperacion(bool Exito, string? Error = null, int Id = 0);
