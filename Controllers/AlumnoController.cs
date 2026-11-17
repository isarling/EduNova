using System.Security.Claims;
using EduNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EduNova.Controllers;
[Authorize(Policy = "alumno.ver")]
public class AlumnoController(ICreditoService service) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return Forbid();
        var dto = await service.EstadoPropioAsync(id);
        if (dto == null) return View("SinVinculo");
        ViewData["EsAlumno"] = true;
        return View("~/Views/Credito/Estado.cshtml", dto);
    }
}
