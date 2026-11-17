using EduNova.DTOS;
using EduNova.Extensions;
using EduNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EduNova.Controllers;
[Authorize(Policy = "matricula.ver")]
public class MatriculaController(IMatriculaService service, ICreditoService credito) : Controller
{
    [Authorize(Policy = "matricula.ver")]
    public async Task<IActionResult> Index(string? nombre)
    {
        var resultado = await service.ConsultarAsync(nombre);
        return View(resultado);
    }
    [Authorize(Policy = "matricula.crear")]
    public async Task<IActionResult> Crear(int? estudianteId)
    {
        var dto = new CrearMatriculaDto
        {
            EstudianteId = estudianteId ?? 0
        };

        await service.PrepararAsync(dto);

        return View(dto);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "matricula.crear")]
    public async Task<IActionResult> Crear(CrearMatriculaDto dto)
    {
        if (dto.PagoInicial > 0 && !User.HasPermission("pago.crear"))
            ModelState.AddModelError("", "Para registrar un pago inicial también necesitas permiso de registrar abonos.");
        if (ModelState.IsValid)
        {
            var result = await service.CrearAsync(dto);
            if (result.Error == null) { TempData["Exito"] = "Matrícula registrada."; return RedirectToAction(nameof(Index)); }
            ModelState.AddModelError(string.Empty, result.Error);
        }
        await service.PrepararAsync(dto);
        return View(dto);
    }
    [HttpGet, Authorize(Policy = "matricula.crear")]
    public async Task<IActionResult> ResumenEstudiante(int id)
    {
        var e = await credito.EstadoAsync(id);
        if (e == null) return NotFound();
        return Ok(new { deuda = e.Deuda, cursosActivos = e.CursosActivos, observacion = e.Observacion,
            descuento = e.Beneficios.FirstOrDefault(b => !b.Utilizado && b.Vence >= ReglasCredito.Hoy)?.Porcentaje ?? 0m });
    }
    [Authorize(Policy = "matricula.ver")]
    public async Task<IActionResult> Detalle(int id)
    {
        var dto = await service.DetalleAsync(id);
        return dto == null ? NotFound() : View(dto);
    }
    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "pago.crear")]
    public async Task<IActionResult> Pagar(RegistrarPagoDto dto)
    {
        if (!ModelState.IsValid) TempData["Error"] = "Revisa los datos del abono.";
        else
        {
            var resultado = await credito.RegistrarPagoAsync(dto);
            if (resultado.Exito && resultado.Id > 0) return RedirectToAction("Recibo", "Documentos", new { id = resultado.Id });
            TempData["Error"] = resultado.Error ?? "No se pudo obtener el recibo.";
        }
        return RedirectToAction(nameof(Detalle), new { id = dto.MatriculaId });
    }
}
