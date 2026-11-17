using EduNova.DTOS;
using EduNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EduNova.Controllers;
[Authorize]
public class CreditoController(ICreditoService service) : Controller
{
    [Authorize(Policy = "credito.configurar")]
    public async Task<IActionResult> Configuracion() => View(await service.PoliticaAsync());
    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "credito.configurar")]
    public async Task<IActionResult> Configuracion(PoliticaCreditoDto dto)
    {
        if (ModelState.IsValid) { var error = await service.GuardarPoliticaAsync(dto); if (error == null) { TempData["Exito"] = "Configuración guardada. Los planes ya contratados conservan sus fechas y costos."; return RedirectToAction(nameof(Configuracion)); } ModelState.AddModelError("", error); }
        return View(dto);
    }
    [Authorize(Policy = "matricula.ver")]
    public async Task<IActionResult> Estado(int id) { var dto = await service.EstadoAsync(id); return dto == null ? NotFound() : View("Estado", dto); }
    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "solvencia.emitir")]
    public async Task<IActionResult> Solvencia(int estudianteId)
    {
        var r = await service.EmitirSolvenciaAsync(estudianteId);
        if (r.Exito) return RedirectToAction("Solvencia", "Documentos", new { id = r.Id });
        TempData["Error"] = r.Error; return RedirectToAction(nameof(Estado), new { id = estudianteId });
    }
    [Authorize(Policy = "estudiante.vincular")]
    public async Task<IActionResult> Cuentas() => View(await service.VinculosAsync());
    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "estudiante.vincular")]
    public async Task<IActionResult> Vincular(VincularCuentaDto dto) { var error = await service.VincularAsync(dto); TempData[error == null ? "Exito" : "Error"] = error ?? "Vínculo guardado."; return RedirectToAction(nameof(Cuentas)); }
    [Authorize(Policy = "catalogo.gestionar")]
    public async Task<IActionResult> Catalogos() => View(await service.CatalogosAsync());
    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "catalogo.gestionar")]
    public async Task<IActionResult> GuardarCatalogo(CatalogoDto dto) { var error = ModelState.IsValid ? await service.GuardarCatalogoAsync(dto) : "Revisa el nombre del catálogo."; TempData[error == null ? "Exito" : "Error"] = error ?? "Catálogo guardado."; return RedirectToAction(nameof(Catalogos)); }
    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "catalogo.gestionar")]
    public async Task<IActionResult> EliminarCatalogo(string tipo, int id) { var error = await service.EliminarCatalogoAsync(tipo, id); TempData[error == null ? "Exito" : "Error"] = error ?? "Registro eliminado."; return RedirectToAction(nameof(Catalogos)); }
}
