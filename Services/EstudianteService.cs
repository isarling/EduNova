using EduNova.DTOS;
using EduNova.Models;
using EduNova.Repositories;

namespace EduNova.Services
{
    public class EstudianteService(
        IEstudianteRepository repo) : IEstudianteService
    {
        public Task<DataTableResponseDto<EstudianteTableDto>> GetPagedAsync(
            DataTablesRequestDto request)
        {
            return repo.GetPagedAsync(request);
        }

        public async Task<IReadOnlyList<EstudianteTableDto>> GetAllAsync()
        {
            var estudiantes = await repo.GetAllAsync();

            return estudiantes.Select(e => new EstudianteTableDto
            {
                Id = e.Id,
                Nombre = e.Nombre ?? string.Empty,
                Apellido = e.Apellido ?? string.Empty,
                Email = e.Email ?? string.Empty,
            }).ToList();
        }

        public async Task<SaveEstudianteDto?> GetByIdAsync(int id)
        {
            var estudiante = await repo.GetByIdAsync(id);

            if (estudiante == null)
                return null;

            return new SaveEstudianteDto
            {
                Id = estudiante.Id,
                Nombre = estudiante.Nombre ?? string.Empty,
                Apellido = estudiante.Apellido ?? string.Empty,
                Email = estudiante.Email ?? string.Empty
            };
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(
    SaveEstudianteDto dto)
        {
            var email = dto.Email.Trim();

            if (await repo.EmailExistsAsync(email))
            {
                return (false, "Ya existe un estudiante con ese correo");
            }

            var estudiante = await repo.CreateAsync(new Estudiante
            {
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                Email = email
            });

            // Conservar el identificador generado al guardar.
            dto.Id = estudiante.Id;

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(
            SaveEstudianteDto dto)
        {
            if (!dto.Id.HasValue)
                return (false, "Id de estudiante inválido");

            var estudiante = await repo.GetByIdAsync(dto.Id.Value);

            if (estudiante == null)
                return (false, "Estudiante no encontrado");

            var email = dto.Email.Trim();

            if (await repo.EmailExistsAsync(email, dto.Id.Value))
                return (false, "Ya existe un estudiante con ese correo");

            estudiante.Nombre = dto.Nombre.Trim();
            estudiante.Apellido = dto.Apellido.Trim();
            estudiante.Email = email;

            var updated = await repo.UpdateAsync(estudiante);

            return updated
                ? (true, null)
                : (false, "No se pudo actualizar el estudiante");
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(
            int id)
        {
            if (await repo.TieneMatriculasAsync(id))
                return (false, "No se puede eliminar un estudiante con matrículas registradas.");
            var deleted = await repo.DeleteAsync(id);

            return deleted
                ? (true, null)
                : (false, "Estudiante no encontrado");
        }
    }
}