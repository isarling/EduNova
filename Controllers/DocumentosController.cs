using System.Security.Claims;
using EduNova.Extensions;
using EduNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EduNova.Controllers;
[Authorize]
public class DocumentosController(ICreditoService service) : Controller
{
    public async Task<IActionResult> Recibo(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId)) return Forbid();
        if (!User.HasAnyPermission("documento.ver", "alumno.ver")) return Forbid();
        var r = await service.ReciboAutorizadoAsync(id, usuarioId, User.HasPermission("documento.ver"));
        return r == null ? NotFound() : View(r);
    }
    public async Task<IActionResult> Solvencia(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId)) return Forbid();
        if (!User.HasAnyPermission("documento.ver", "alumno.ver")) return Forbid();
        var c = await service.SolvenciaAutorizadaAsync(id, usuarioId, User.HasPermission("documento.ver"));
        return c == null ? NotFound() : View(c);
    }
}
