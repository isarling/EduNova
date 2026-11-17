using EduNova.DTOS;
using EduNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduNova.Controllers
{
    public class CursoController(ICursoService cursoService, ICreditoService credito) : Controller
    {
        [Authorize(Policy = "curso.ver")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Catalogos = await credito.CatalogosAsync();
            return View();
        }

        [Authorize(Policy = "curso.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData(DataTablesRequestDto request)
        {
            var result = await cursoService.GetPagedAsync(request);

            return Ok(result);
        }

        [Authorize(Policy = "curso.ver")]
        [HttpGet]
        public async Task<IActionResult> GetById(int Id)
        {
            var result = await cursoService.GetByIdAsync(Id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Curso no encontrado"
                });
            }

            return Ok(result);
        }

        [Authorize(Policy = "curso.crear")]
        [HttpPost]
        public async Task<IActionResult> Create(SaveCursoDto cursoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Error en los datos a guardar"
                });
            }

            var (success, errorMessage) =
                await cursoService.CreateAsync(cursoDto);

            return success
                ? Ok(new
                {
                    success = true,
                    message = "Curso creado correctamente"
                })
                : BadRequest(new
                {
                    success = false,
                    message = errorMessage
                });
        }

        [Authorize(Policy = "curso.actualizar")]
        [HttpPost]
        public async Task<IActionResult> Update(SaveCursoDto cursoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Error en los datos a guardar"
                });
            }

            var (success, errorMessage) =
                await cursoService.UpdateAsync(cursoDto);

            return success
                ? Ok(new
                {
                    success = true,
                    message = "Curso actualizado correctamente"
                })
                : BadRequest(new
                {
                    success = false,
                    message = errorMessage
                });
        }

        [Authorize(Policy = "curso.borrar")]
        [HttpPost]
        public async Task<IActionResult> Delete(int Id)
        {
            var (success, errorMessage) =
                await cursoService.DeleteAsync(Id);

            return success
                ? Ok(new
                {
                    success = true,
                    message = "Curso eliminado correctamente"
                })
                : BadRequest(new
                {
                    success = false,
                    message = errorMessage
                });
        }
    }
}