using EduNova.DTOS;
using EduNova.Repositories;
namespace EduNova.Services;
public interface IMatriculaService
{
    Task<List<MatriculaResumenDto>> ListarAsync();
    Task<ConsultaMatriculasDto> ConsultarAsync(string? nombre);
    Task PrepararAsync(CrearMatriculaDto dto);
    Task<(int Id, string? Error)> CrearAsync(CrearMatriculaDto dto);
    Task<MatriculaDetalleDto?> DetalleAsync(int id);
    Task<string?> PagarAsync(RegistrarPagoDto dto);
}
public class MatriculaService(ICreditoRepository repo, ICreditoService credito) : IMatriculaService
{
    public async Task<List<MatriculaResumenDto>> ListarAsync() => (await repo.MatriculasAsync()).Select(CreditoService.Resumen).ToList();

    public async Task<ConsultaMatriculasDto> ConsultarAsync(string? nombre)
    {
        var busqueda = (nombre ?? string.Empty).Trim();

        var matriculas = (await repo.MatriculasAsync())
            .OrderBy(m => m.Id)
            .ToList();

        var resultado = new ConsultaMatriculasDto
        {
            Busqueda = busqueda
        };

        // Sin búsqueda, mostrar todas las matrículas en orden ascendente.
        if (busqueda.Length == 0)
        {
            resultado.Matriculas = matriculas
                .Select(CreditoService.Resumen)
                .ToList();

            return resultado;
        }

        // Buscar por nombre o apellido, sin distinguir mayúsculas ni acentos.
        var comparador = System.Globalization.CultureInfo
            .GetCultureInfo("es-NI")
            .CompareInfo;

        var estudiantes = (await repo.EstudiantesAsync())
            .Where(e => comparador.IndexOf(
                $"{e.Nombre} {e.Apellido}",
                busqueda,
                System.Globalization.CompareOptions.IgnoreCase |
                System.Globalization.CompareOptions.IgnoreNonSpace) >= 0);

        foreach (var estudiante in estudiantes)
        {
            var propias = matriculas
                .Where(m => m.EstudianteId == estudiante.Id)
                .ToList();

            var persona = new ConsultaPersonaDto
            {
                EstudianteId = estudiante.Id,
                Nombre = $"{estudiante.Nombre} {estudiante.Apellido}",

                CantidadCursos = propias
                    .SelectMany(m => m.Detalles)
                    .Count(),

                Total = propias.Sum(m =>
                    m.Detalles.Sum(d => d.Costo)),

                Pagado = propias.Sum(m =>
                    m.Pagos.Sum(p => p.Monto)),

                Cursos = propias.SelectMany(m =>
                    m.Detalles.Select(d => new CursoInscritoDto
                    {
                        MatriculaId = m.Id,
                        Nombre = d.Curso.Nombre,
                        Fecha = m.Fecha,
                        Costo = d.Costo,
                        Pagado = d.Cuotas.Sum(c => c.Pagado)
                    }))
                    .ToList(),

                Cuotas = propias.SelectMany(m =>
                    m.Detalles.SelectMany(d =>
                        d.Cuotas.Select(c => new CuotaDto
                        {
                            MatriculaId = m.Id,
                            Curso = d.Curso.Nombre,
                            Numero = c.Numero,
                            Vencimiento = c.Vencimiento,
                            Monto = c.Monto,
                            Pagado = c.Pagado
                        })))
                    .OrderBy(c => c.Vencimiento)
                    .ThenBy(c => c.MatriculaId)
                    .ThenBy(c => c.Numero)
                    .ToList(),

                Pagos = propias
                    .SelectMany(m => m.Pagos)
                    .OrderBy(p => p.Fecha)
                    .Select(p => new PagoResumenDto
                    {
                        Fecha = p.Fecha,
                        Monto = p.Monto,
                        Referencia = p.Referencia,
                        ReciboId = p.Recibo?.Id
                    })
                    .ToList()
            };

            resultado.Personas.Add(persona);
        }

        return resultado;
    }
    public async Task PrepararAsync(CrearMatriculaDto dto)
    {
        dto.Politica = await repo.PoliticaAsync();
        dto.Estudiantes = (await repo.EstudiantesAsync()).Select(e => new OpcionAcademiaDto { Id = e.Id, Nombre = $"{e.Nombre} {e.Apellido}" }).ToList();
        var previas = dto.Lineas ?? new();
        dto.Lineas = (await repo.CursosAsync()).Select(c => new LineaMatriculaDto { CursoId = c.Id, Nombre = c.Nombre, Costo = c.Precio, Seleccionado = previas.Any(l => l.CursoId == c.Id && l.Seleccionado), CantidadCuotas = previas.FirstOrDefault(l => l.CursoId == c.Id)?.CantidadCuotas ?? 1 }).ToList();
    }
    public async Task<(int Id, string? Error)> CrearAsync(CrearMatriculaDto dto) { var r = await credito.CrearMatriculaAsync(dto); return (r.Id, r.Error); }
    public async Task<string?> PagarAsync(RegistrarPagoDto dto) => (await credito.RegistrarPagoAsync(dto)).Error;
    public async Task<MatriculaDetalleDto?> DetalleAsync(int id)
    {
        var m = await repo.MatriculaAsync(id); if (m == null) return null;
        return new() { Resumen = CreditoService.Resumen(m), EstudianteId = m.EstudianteId,
            Cursos = m.Detalles.Select(d => new LineaMatriculaDto { CursoId = d.CursoId, Nombre = d.Curso.Nombre, Costo = d.Costo }).ToList(),
            Cuotas = m.Detalles.SelectMany(d => d.Cuotas.Select(c => new CuotaDto { MatriculaId = m.Id, Curso = d.Curso.Nombre, Numero = c.Numero, Vencimiento = c.Vencimiento, Monto = c.Monto, Pagado = c.Pagado })).OrderBy(c => c.Vencimiento).ToList(),
            Pagos = m.Pagos.OrderByDescending(p => p.Fecha).Select(p => new PagoResumenDto { ReciboId = p.Recibo?.Id, Fecha = p.Fecha, Monto = p.Monto, Referencia = p.Referencia }).ToList(),
            NuevoPago = new() { MatriculaId = id, OperacionId = Guid.NewGuid() } };
    }
}
