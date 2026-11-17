using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduNova.DTOS;
using EduNova.Services;

namespace EduNova.Controllers
{
    public class EstudianteController(
        IEstudianteService _estudianteService) : Controller
    {
        [Authorize(Policy = "estudiante.ver")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "estudiante.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData(
            DataTablesRequestDto request)
        {
            var result = await _estudianteService.GetPagedAsync(request);
            return Ok(result);
        }

        [Authorize(Policy = "estudiante.ver")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _estudianteService.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Estudiante no encontrado"
                });
            }

            return Ok(result);
        }

        [Authorize(Policy = "estudiante.crear")]
        [HttpPost]
        public async Task<IActionResult> Create(SaveEstudianteDto dto)
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
                await _estudianteService.CreateAsync(dto);

            return success
                ? Ok(new
                {
                    success = true,
                    message = "Estudiante creado satisfactoriamente"
                })
                : BadRequest(new
                {
                    success = false,
                    message = errorMessage
                });
        }

        [HttpGet]
        [Authorize(Policy = "estudiante.crear")]
        [Authorize(Policy = "matricula.crear")]
        [Authorize(Policy = "matricula.ver")]
        public IActionResult NuevoParaMatricula()
        {
            return View(new SaveEstudianteDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "estudiante.crear")]
        [Authorize(Policy = "matricula.crear")]
        [Authorize(Policy = "matricula.ver")]
        public async Task<IActionResult> NuevoParaMatricula(
            SaveEstudianteDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var (success, errorMessage) =
                await _estudianteService.CreateAsync(dto);

            if (!success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    errorMessage ?? "No se pudo registrar al estudiante.");

                return View(dto);
            }

            TempData["Exito"] =
                "Estudiante registrado. Ahora selecciona sus cursos para completar la matrícula.";

            return RedirectToAction(
                "Crear",
                "Matricula",
                new { estudianteId = dto.Id });
        }

        [Authorize(Policy = "estudiante.actualizar")]
        [HttpPost]
        public async Task<IActionResult> Update(SaveEstudianteDto dto)
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
                await _estudianteService.UpdateAsync(dto);

            return success
                ? Ok(new
                {
                    success = true,
                    message = "Estudiante actualizado satisfactoriamente"
                })
                : BadRequest(new
                {
                    success = false,
                    message = errorMessage
                });
        }

        [Authorize(Policy = "estudiante.borrar")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errorMessage) =
                await _estudianteService.DeleteAsync(id);

            return success
                ? Ok(new
                {
                    success = true,
                    message = "Estudiante eliminado satisfactoriamente"
                })
                : BadRequest(new
                {
                    success = false,
                    message = errorMessage
                });
        }
    }
}