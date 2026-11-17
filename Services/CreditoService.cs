using EduNova.DTOS;
using EduNova.Models;
using EduNova.Repositories;
using System.ComponentModel.DataAnnotations;
namespace EduNova.Services;
public class CreditoService(ICreditoRepository repo) : ICreditoService
{
    public static decimal Total(Matricula m) => m.Detalles.Sum(d => d.Costo);
    public static decimal Saldo(Matricula m) => Total(m) - m.Pagos.Sum(p => p.Monto);
    public static MatriculaResumenDto Resumen(Matricula m) => new() { Id = m.Id, Estudiante = $"{m.Estudiante.Nombre} {m.Estudiante.Apellido}", Fecha = m.Fecha, Total = Total(m), Pagado = m.Pagos.Sum(p => p.Monto) };
    private static bool EsDinero(decimal x) => x >= 0 && x <= 99999999.99m && ReglasCredito.Dinero(x) == x;
    private static bool Atraso(IEnumerable<Matricula> ms, PoliticaCredito p) => ms.SelectMany(m => m.Detalles).SelectMany(d => d.Cuotas).Any(c => c.Pagado < c.Monto && c.Vencimiento.Date.AddDays(p.DiasGracia) < ReglasCredito.Hoy);
    public async Task<ResultadoOperacion> CrearMatriculaAsync(CrearMatriculaDto dto)
    {
        if (dto.OperacionId == Guid.Empty || !EsDinero(dto.PagoInicial)) return new(false, "Revisa el pago inicial y vuelve a abrir el formulario.");
        await using var tx = await repo.BloquearEstudianteAsync(dto.EstudianteId);
        var estudiante = (await repo.EstudiantesAsync()).SingleOrDefault(e => e.Id == dto.EstudianteId);
        if (estudiante == null) return new(false, "El estudiante no existe.");
        var anterior = await repo.MatriculaOperacionAsync(dto.OperacionId);
        if (anterior != null) return anterior.EstudianteId == dto.EstudianteId ? new(true, Id: anterior.Id) : new(false, "Esta operación ya fue utilizada.");
        var ms = await repo.MatriculasAsync(estudiante.Id);
        var p = await repo.PoliticaAsync();
        var cursos = await repo.CursosAsync();
        var lineas = (dto.Lineas ?? new()).Where(l => l.Seleccionado).ToList();
        if (lineas.Count == 0) return new(false, "Selecciona al menos un curso.");
        if (lineas.Select(l => l.CursoId).Distinct().Count() != lineas.Count) return new(false, "No se puede repetir un curso en la misma matrícula.");
        if (lineas.Any(l => !cursos.Any(c => c.Id == l.CursoId && c.Precio > 0))) return new(false, "Hay cursos inválidos o sin precio configurado.");
        if (lineas.Any(l => l.CantidadCuotas < 1 || l.CantidadCuotas > p.MaxCuotas)) return new(false, $"Cada curso permite entre 1 y {p.MaxCuotas} cuotas.");
        var activos = ms.SelectMany(m => m.Detalles).Where(d => d.FechaFin.Date >= ReglasCredito.Hoy).ToList();
        if (lineas.Any(l => activos.Any(a => a.CursoId == l.CursoId))) return new(false, "El estudiante ya tiene activo uno de los cursos elegidos.");
        var beneficios = await repo.BeneficiosAsync(estudiante.Id);
        var beneficio = dto.AplicarBeneficio ? beneficios.FirstOrDefault(b => b.MatriculaDestinoId == null && b.Vence >= ReglasCredito.Hoy) : null;
        if (dto.AplicarBeneficio && beneficio == null) return new(false, "No hay un descuento vigente sin utilizar.");
        var matricula = new Matricula { Estudiante = estudiante, EstudianteId = estudiante.Id, OperacionId = dto.OperacionId, Fecha = DateTime.UtcNow };
        foreach (var linea in lineas)
        {
            var curso = cursos.Single(c => c.Id == linea.CursoId);
            var descuento = ReglasCredito.Dinero(curso.Precio * (beneficio?.Porcentaje ?? 0) / 100m);
            var costo = curso.Precio - descuento;
            if (costo <= 0 || costo * 100 < linea.CantidadCuotas) return new(false, "El costo no permite distribuir ese número de cuotas.");
            var detalle = new DetalleMatricula { Curso = curso, CursoId = curso.Id, Costo = costo, PrecioCatalogo = curso.Precio, Descuento = descuento, FechaFin = ReglasCredito.Hoy.AddDays(curso.DuracionDias), Cuotas = ReglasCredito.CrearCuotas(costo, linea.CantidadCuotas, ReglasCredito.Hoy, p.DiasEntreCuotas) };
            matricula.Detalles.Add(detalle);
        }
        var error = ReglasCredito.ValidarNueva(p, ms.Sum(Saldo), Total(matricula), dto.PagoInicial, activos.Count, lineas.Count, Atraso(ms, p));
        if (error != null) return new(false, error);
        matricula.MinimoInicialAplicado = ReglasCredito.Dinero(Total(matricula) * p.PorcentajeInicial / 100m);
        repo.Agregar(matricula);
        if (beneficio != null) beneficio.MatriculaDestino = matricula;
        await repo.GuardarAsync();
        if (dto.PagoInicial > 0)
        {
            // El pago inicial y la matrícula se confirman en la misma transacción.
            AplicarPago(matricula, dto.PagoInicial, "Pago inicial", Guid.NewGuid());
            await repo.GuardarAsync();
        }
        await ConcederBeneficioAsync(matricula, p);
        await repo.GuardarAsync();
        await CrearSolvenciaAutomaticaAsync(estudiante.Id);
        await repo.GuardarAsync();
        await tx.CommitAsync();
        return new(true, Id: matricula.Id);
    }
    private Pago AplicarPago(Matricula m, decimal monto, string referencia, Guid operacion)
    {
        var distribucion = ReglasCredito.Distribuir(m.Detalles.SelectMany(d => d.Cuotas), monto);
        var pago = new Pago { Matricula = m, MatriculaId = m.Id, Monto = monto, Referencia = referencia, OperacionId = operacion, Fecha = DateTime.UtcNow };
        var recibo = new ReciboPago { Pago = pago, Fecha = pago.Fecha, NombreEstudiante = $"{m.Estudiante.Nombre} {m.Estudiante.Apellido}", Monto = monto, SaldoPosterior = Saldo(m) - monto };
        foreach (var (cuota, parte) in distribucion)
        {
            cuota.Pagado += parte;
            pago.Detalles.Add(new DetallePago { Cuota = cuota, CuotaId = cuota.Id, Monto = parte });
            recibo.Detalles.Add(new DetalleRecibo { Curso = cuota.DetalleMatricula.Curso.Nombre, NumeroCuota = cuota.Numero, Vencimiento = cuota.Vencimiento, Monto = parte });
        }
        pago.Recibo = recibo;
        repo.Agregar(pago);
        // EF fija las relaciones; añadirlo a la colección mantiene el saldo calculado en memoria.
        if (!m.Pagos.Contains(pago)) m.Pagos.Add(pago);
        return pago;
    }
    private async Task ConcederBeneficioAsync(Matricula m, PoliticaCredito p)
    {
        if (Saldo(m) != 0 || p.PorcentajeBeneficio <= 0) return;
        var cuotas = m.Detalles.SelectMany(d => d.Cuotas).ToList();
        if (cuotas.Count == 0 || ReglasCredito.Hoy >= cuotas.Max(c => c.Vencimiento).Date) return;
        if ((await repo.BeneficiosAsync(m.EstudianteId)).Any(b => b.MatriculaOrigenId == m.Id)) return;
        repo.Agregar(new BeneficioDescuento { EstudianteId = m.EstudianteId, MatriculaOrigenId = m.Id, Emitido = ReglasCredito.Hoy, Vence = ReglasCredito.Hoy.AddDays(p.DiasVigenciaBeneficio), Porcentaje = p.PorcentajeBeneficio });
    }
    public async Task<ResultadoOperacion> RegistrarPagoAsync(RegistrarPagoDto dto)
    {
        if (!EsDinero(dto.Monto) || dto.Monto <= 0 || dto.OperacionId == Guid.Empty || (dto.Referencia?.Length ?? 0) > 100) return new(false, "Revisa el monto y la referencia del pago.");
        var propietario = await repo.PropietarioMatriculaAsync(dto.MatriculaId);
        if (propietario == null) return new(false, "La matrícula no existe.");
        await using var tx = await repo.BloquearEstudianteAsync(propietario.Value);
        var m = await repo.MatriculaAsync(dto.MatriculaId);
        if (m == null) return new(false, "La matrícula no existe.");
        var anterior = await repo.PagoOperacionAsync(dto.OperacionId);
        if (anterior != null) return anterior.MatriculaId == m.Id && anterior.Monto == dto.Monto ? new(true, Id: anterior.Recibo?.Id ?? 0) : new(false, "Esta operación de pago ya fue utilizada.");
        var p = await repo.PoliticaAsync();
        var error = ReglasCredito.ValidarAbono(p, Saldo(m), dto.Monto);
        if (error != null) return new(false, error);
        if (m.Detalles.SelectMany(d => d.Cuotas).Sum(c => c.Monto - c.Pagado) != Saldo(m)) return new(false, "El plan de cuotas no coincide con el saldo. Solicita revisar la migración de los registros anteriores.");
        var pago = AplicarPago(m, dto.Monto, dto.Referencia?.Trim() ?? string.Empty, dto.OperacionId);
        await repo.GuardarAsync();
        await ConcederBeneficioAsync(m, p);
        await repo.GuardarAsync();
        await CrearSolvenciaAutomaticaAsync(m.EstudianteId);
        await repo.GuardarAsync();
        await tx.CommitAsync();
        return new(true, Id: pago.Recibo!.Id);
    }
    private async Task CrearSolvenciaAutomaticaAsync(int estudianteId)
    {
        var ms = await repo.MatriculasAsync(estudianteId);
        if (ms.Count == 0 || ms.Sum(Saldo) != 0) return;
        var e = ms[0].Estudiante;
        repo.Agregar(new ConstanciaSolvencia { EstudianteId = e.Id, NombreEstudiante = $"{e.Nombre} {e.Apellido}", Fecha = DateTime.UtcNow, TotalContratado = ms.Sum(Total), TotalPagado = ms.Sum(m => m.Pagos.Sum(p => p.Monto)), Detalles = ms.Select(m => new DetalleSolvencia { MatriculaId = m.Id, Total = Total(m), Pagado = m.Pagos.Sum(p => p.Monto) }).ToList() });
    }
    public async Task<EstadoEstudianteDto?> EstadoAsync(int estudianteId)
    {
        var e = (await repo.EstudiantesAsync()).SingleOrDefault(e => e.Id == estudianteId);
        if (e == null) return null;
        var ms = await repo.MatriculasAsync(e.Id);
        var p = await repo.PoliticaAsync();
        var atraso = Atraso(ms, p);
        var deuda = ms.Sum(Saldo);
        var activos = ms.SelectMany(m => m.Detalles).Count(d => d.FechaFin.Date >= ReglasCredito.Hoy);
        var limite = deuda > 0 ? p.MaxCursosConDeuda : p.MaxCursosSinDeuda;
        return new EstadoEstudianteDto
        {
            EstudianteId = e.Id, Nombre = $"{e.Nombre} {e.Apellido}", Deuda = deuda, CursosActivos = activos, TieneAtraso = atraso,
            Observacion = atraso ? "Nuevas matrículas bloqueadas por atraso." : deuda > p.DeudaMaxima ? "Nuevas matrículas bloqueadas por deuda." : activos >= limite ? "Se alcanzó el límite de cursos activos." : "Puede solicitar otra inscripción, sujeta a los límites y al pago inicial.",
            Matriculas = ms.Select(Resumen).ToList(),
            Beneficios = (await repo.BeneficiosAsync(e.Id)).Select(b => new BeneficioDto { Porcentaje = b.Porcentaje, Vence = b.Vence, Utilizado = b.MatriculaDestinoId != null }).ToList(),
            Cuotas = ms.SelectMany(m => m.Detalles.SelectMany(d => d.Cuotas.Select(c => new CuotaDto { MatriculaId = m.Id, Curso = d.Curso.Nombre, Numero = c.Numero, Vencimiento = c.Vencimiento, Monto = c.Monto, Pagado = c.Pagado }))).OrderBy(c => c.Vencimiento).ToList(),
            Recibos = ms.SelectMany(m => m.Pagos).Where(p => p.Recibo != null).OrderByDescending(p => p.Fecha).Select(p => new OpcionAcademiaDto { Id = p.Recibo!.Id, Nombre = $"REC-{p.Recibo.Id:D6} · C$ {p.Monto:N2} · {p.Fecha.AddHours(-6):dd/MM/yyyy}" }).ToList(),
            Constancias = (await repo.ConstanciasAsync(e.Id)).Select(c => new OpcionAcademiaDto { Id = c.Id, Nombre = $"SOL-{c.Id:D6} · {c.Fecha.AddHours(-6):dd/MM/yyyy}" }).ToList()
        };
    }
    public async Task<EstadoEstudianteDto?> EstadoPropioAsync(int usuarioId)
    {
        var e = await repo.EstudianteDeUsuarioAsync(usuarioId);
        return e == null ? null : await EstadoAsync(e.Id);
    }
    public async Task<ResultadoOperacion> EmitirSolvenciaAsync(int estudianteId)
    {
        await using var tx = await repo.BloquearEstudianteAsync(estudianteId);
        var ms = await repo.MatriculasAsync(estudianteId);
        if (ms.Count == 0) return new(false, "El estudiante no tiene matrículas registradas.");
        if (ms.Sum(Saldo) != 0) return new(false, "No se puede emitir solvencia mientras exista deuda en alguna matrícula.");
        var e = ms[0].Estudiante;
        var c = new ConstanciaSolvencia { EstudianteId = e.Id, NombreEstudiante = $"{e.Nombre} {e.Apellido}", Fecha = DateTime.UtcNow, TotalContratado = ms.Sum(Total), TotalPagado = ms.Sum(m => m.Pagos.Sum(p => p.Monto)), Detalles = ms.Select(m => new DetalleSolvencia { MatriculaId = m.Id, Total = Total(m), Pagado = m.Pagos.Sum(p => p.Monto) }).ToList() };
        repo.Agregar(c); await repo.GuardarAsync(); await tx.CommitAsync();
        return new(true, Id: c.Id);
    }
    public async Task<ReciboPago?> ReciboAutorizadoAsync(int id, int usuarioId, bool personal)
    {
        var r = await repo.ReciboAsync(id);
        return r != null && (personal || r.Pago.Matricula.Estudiante.UsuarioId == usuarioId) ? r : null;
    }
    public async Task<ConstanciaSolvencia?> SolvenciaAutorizadaAsync(int id, int usuarioId, bool personal)
    {
        var c = await repo.ConstanciaAsync(id);
        return c != null && (personal || c.Estudiante.UsuarioId == usuarioId) ? c : null;
    }
    public async Task<PoliticaCreditoDto> PoliticaAsync()
    {
        var p = await repo.PoliticaAsync();
        return new() { DeudaMaxima = p.DeudaMaxima, MaxCursosSinDeuda = p.MaxCursosSinDeuda, MaxCursosConDeuda = p.MaxCursosConDeuda, PorcentajeInicial = p.PorcentajeInicial, AbonoMinimo = p.AbonoMinimo, MaxCuotas = p.MaxCuotas, DiasEntreCuotas = p.DiasEntreCuotas, DiasGracia = p.DiasGracia, PorcentajeBeneficio = p.PorcentajeBeneficio, DiasVigenciaBeneficio = p.DiasVigenciaBeneficio };
    }
    public async Task<string?> GuardarPoliticaAsync(PoliticaCreditoDto dto)
    {
        if (!Validator.TryValidateObject(dto, new ValidationContext(dto), new List<ValidationResult>(), true)) return "Hay valores fuera de los límites permitidos.";
        if (dto.MaxCursosConDeuda > dto.MaxCursosSinDeuda) return "El límite con deuda no puede superar el límite sin deuda.";
        if (new[] { dto.DeudaMaxima, dto.PorcentajeInicial, dto.AbonoMinimo, dto.PorcentajeBeneficio }.Any(v => ReglasCredito.Dinero(v) != v)) return "Usa como máximo dos decimales.";
        var p = await repo.PoliticaAsync();
        p.DeudaMaxima = dto.DeudaMaxima; p.MaxCursosSinDeuda = dto.MaxCursosSinDeuda; p.MaxCursosConDeuda = dto.MaxCursosConDeuda; p.PorcentajeInicial = dto.PorcentajeInicial; p.AbonoMinimo = dto.AbonoMinimo; p.MaxCuotas = dto.MaxCuotas; p.DiasEntreCuotas = dto.DiasEntreCuotas; p.DiasGracia = dto.DiasGracia; p.PorcentajeBeneficio = dto.PorcentajeBeneficio; p.DiasVigenciaBeneficio = dto.DiasVigenciaBeneficio;
        await repo.GuardarAsync(); return null;
    }
    public async Task<VincularVistaDto> VinculosAsync() => new() { Estudiantes = (await repo.EstudiantesAsync()).Select(e => new OpcionAcademiaDto { Id = e.Id, Nombre = $"{e.Nombre} {e.Apellido}" }).ToList(), Usuarios = (await repo.UsuariosAsync()).Where(u => u.Rol?.Name == "Estudiante").Select(u => new OpcionAcademiaDto { Id = u.Id, Nombre = $"{u.UserName} · {u.Email}" }).ToList(), Vinculos = (await repo.EstudiantesAsync()).ToDictionary(e => e.Id, e => e.UsuarioId) };
    public async Task<string?> VincularAsync(VincularCuentaDto dto)
    {
        await using var tx = await repo.TransaccionAsync();
        var es = await repo.EstudiantesAsync();
        var e = es.SingleOrDefault(e => e.Id == dto.EstudianteId);
        if (e == null) return "El estudiante no existe.";
        if (dto.UsuarioId.HasValue)
        {
            var u = (await repo.UsuariosAsync()).SingleOrDefault(u => u.Id == dto.UsuarioId && u.Rol?.Name == "Estudiante");
            if (u == null) return "La cuenta debe tener el rol Estudiante.";
            if (es.Any(x => x.Id != e.Id && x.UsuarioId == u.Id)) return "La cuenta ya está vinculada a otro estudiante.";
        }
        e.UsuarioId = dto.UsuarioId; await repo.GuardarAsync(); await tx.CommitAsync(); return null;
    }
    public async Task<CatalogosVistaDto> CatalogosAsync() => new() { Categorias = (await repo.CategoriasAsync()).Select(c => new OpcionAcademiaDto { Id = c.Id, Nombre = c.Nombre }).ToList(), Modalidades = (await repo.ModalidadesAsync()).Select(m => new OpcionAcademiaDto { Id = m.Id, Nombre = m.Nombre }).ToList() };
    public async Task<string?> GuardarCatalogoAsync(CatalogoDto dto)
    {
        var nombre = dto.Nombre?.Trim() ?? "";
        if (nombre.Length < 2 || nombre.Length > 80) return "El nombre debe tener entre 2 y 80 caracteres.";
        if (dto.Tipo == "categoria")
        {
            var cs = await repo.CategoriasAsync();
            if (cs.Any(c => c.Id != dto.Id && c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase))) return "El nombre ya existe.";
            if (dto.Id == 0) repo.Agregar(new CategoriaCurso { Nombre = nombre });
            else { var c = cs.SingleOrDefault(c => c.Id == dto.Id); if (c == null) return "Categoría no encontrada."; c.Nombre = nombre; }
        }
        else if (dto.Tipo == "modalidad")
        {
            var ms = await repo.ModalidadesAsync();
            if (ms.Any(m => m.Id != dto.Id && m.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase))) return "El nombre ya existe.";
            if (dto.Id == 0) repo.Agregar(new ModalidadCurso { Nombre = nombre });
            else { var m = ms.SingleOrDefault(m => m.Id == dto.Id); if (m == null) return "Modalidad no encontrada."; m.Nombre = nombre; }
        }
        else return "Tipo de catálogo inválido.";
        await repo.GuardarAsync(); return null;
    }
    public async Task<string?> EliminarCatalogoAsync(string tipo, int id)
    {
        if (tipo == "categoria") { var c = (await repo.CategoriasAsync()).SingleOrDefault(c => c.Id == id); if (c == null) return "Categoría no encontrada."; if (c.Cursos.Count > 0) return "La categoría está asociada a cursos."; repo.Eliminar(c); }
        else if (tipo == "modalidad") { var m = (await repo.ModalidadesAsync()).SingleOrDefault(m => m.Id == id); if (m == null) return "Modalidad no encontrada."; if (m.Cursos.Count > 0) return "La modalidad está asociada a cursos."; repo.Eliminar(m); }
        else return "Tipo inválido.";
        await repo.GuardarAsync(); return null;
    }
}
