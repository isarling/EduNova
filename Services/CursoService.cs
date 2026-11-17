using EduNova.DTOS;
using EduNova.Models;
using EduNova.Repositories;

namespace EduNova.Services
{
    public class CursoService(ICursoRepository repo) : ICursoService
    {
        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveCursoDto dto)
        {
            if (dto.DuracionDias < 1 || dto.DuracionDias > 730) return (false, "La duración debe estar entre 1 y 730 días.");
            if (!await repo.CategoriaValidaAsync(dto.CategoriaCursoId) || !await repo.ModalidadValidaAsync(dto.ModalidadCursoId)) return (false, "La categoría o modalidad no existe.");
            if (dto.Precio <= 0 || dto.Precio > 99999999.99m || decimal.Round(dto.Precio, 2) != dto.Precio)
                return (false, "Ingresa un precio positivo con máximo dos decimales.");
            if (await repo.NombreExisteAsync(dto.Nombre.Trim()))
                return (false, "El nombre del curso ya existe");
            await repo.CreateAsync(new Curso
            {
                Nombre = dto.Nombre.Trim(),
                Precio = dto.Precio,
                DuracionDias = dto.DuracionDias,
                CategoriaCursoId = dto.CategoriaCursoId,
                ModalidadCursoId = dto.ModalidadCursoId,
                Descripcion = dto.Descripcion?.Trim() ?? string.Empty,

            });
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            if (await repo.HasAnyEstudentAsync(id))
                return (false, "No se puede eliminar el curso porque tiene estudiantes o matrículas asociadas");
            var deleted = await repo.DeleteAsync(id);
            return deleted ? (true, null) : (false, "Curso no encontrado");
        }

        public async Task<IReadOnlyList<CursoTableDto>> GetAllAsync()
        {
            var cursos = await repo.GetAllAsync();
            return cursos.Select(c => new CursoTableDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                Precio = c.Precio,
            }).ToList();
        }

        public async  Task<SaveCursoDto?> GetByIdAsync(int id)
        {
            var curso = await repo.GetByIdAsync(id);
            if (curso == null) return null;
            return new SaveCursoDto
            {
                Id = curso.Id,
                Nombre = curso.Nombre,
                Descripcion = curso.Descripcion,
                Precio = curso.Precio,
                DuracionDias = curso.DuracionDias,
                CategoriaCursoId = curso.CategoriaCursoId,
                ModalidadCursoId = curso.ModalidadCursoId,
            };
        }

        public Task<DataTableResponseDto<CursoTableDto>> GetPagedAsync(DataTablesRequestDto request)
        => repo.GetPagedAsync(request); 

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveCursoDto dto)
        {
            if (dto.DuracionDias < 1 || dto.DuracionDias > 730) return (false, "La duración debe estar entre 1 y 730 días.");
            if (!await repo.CategoriaValidaAsync(dto.CategoriaCursoId) || !await repo.ModalidadValidaAsync(dto.ModalidadCursoId)) return (false, "La categoría o modalidad no existe.");
            if (dto.Precio <= 0 || dto.Precio > 99999999.99m || decimal.Round(dto.Precio, 2) != dto.Precio)
                return (false, "Ingresa un precio positivo con máximo dos decimales.");
            if (!dto.Id.HasValue)
                return (false, "Id invalido");

            var curso = await repo.GetByIdAsync(dto.Id.Value);
            if (curso == null)
                return (false, "Curso no encontrado");

            if (await repo.NombreExisteAsync(dto.Nombre.Trim(), dto.Id))
                return (false, "Ya existe un curso con ese nombre");
            curso.Nombre = dto.Nombre.Trim();
            curso.Precio = dto.Precio;
            curso.DuracionDias = dto.DuracionDias;
            curso.CategoriaCursoId = dto.CategoriaCursoId;
            curso.ModalidadCursoId = dto.ModalidadCursoId;
            curso.Descripcion = dto.Descripcion?.Trim() ?? string.Empty;
            await repo.UpdateAsync(curso);
            return (true, null);
        }
    }
}
