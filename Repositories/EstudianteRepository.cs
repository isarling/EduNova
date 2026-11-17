using EduNova.Data;
using EduNova.DTOS;
using EduNova.Models;
using Microsoft.EntityFrameworkCore;

namespace EduNova.Repositories
{
    public class EstudianteRepository(AppDBContext context) : IEstudianteRepository
    {
        public async Task<IReadOnlyList<Estudiante>> GetAllAsync()
        {
            return await context.Estudiantes
                .OrderBy(e => e.Nombre)
                .ThenBy(e => e.Apellido)
                .ToListAsync();
        }

        public async Task<Estudiante> CreateAsync(Estudiante estudiante)
        {
            var entry = await context.Estudiantes.AddAsync(estudiante);
            await context.SaveChangesAsync();
            return entry.Entity;
        }

        public Task<bool> TieneMatriculasAsync(int id) => context.Matriculas.AnyAsync(m => m.EstudianteId == id);

        public async Task<bool> DeleteAsync(int Id)
        {
            var estudiante = await context.Estudiantes.FindAsync(Id);

            if (estudiante == null)
                return false;

            context.Estudiantes.Remove(estudiante);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EmailExistsAsync(
            string email,
            int? ExcludeId = null)
        {
            return await context.Estudiantes.AnyAsync(e =>
                e.Email == email &&
                (!ExcludeId.HasValue || e.Id != ExcludeId.Value));
        }

        public async Task<Estudiante?> GetByIdAsync(int Id)
        {
            return await context.Estudiantes
                .FirstOrDefaultAsync(e => e.Id == Id);
        }

        public async Task<DataTableResponseDto<EstudianteTableDto>> GetPagedAsync(
            DataTablesRequestDto request)
        {
            var query = context.Estudiantes
                .AsNoTracking()
                .AsQueryable();

            var recordsTotal = await query.CountAsync();
            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e =>
                    (e.Nombre != null && e.Nombre.Contains(search)) ||
                    (e.Apellido != null && e.Apellido.Contains(search)) ||
                    (e.Email != null && e.Email.Contains(search)));
            }

            var recordsFiltered = await query.CountAsync();
            var order = request.Order?.FirstOrDefault();
            var orderColumn = order?.Column ?? 0;
            var orderDir = order?.Dir ?? "asc";

            var columnName = orderColumn >= 0
                ? request.Columns?.ElementAtOrDefault(orderColumn)?.Data
                : null;

            var descending = orderDir.Equals(
                "desc",
                StringComparison.OrdinalIgnoreCase);

            switch ((columnName ?? "nombre").ToLowerInvariant())
            {
                case "apellido":
                    query = descending
                        ? query.OrderByDescending(e => e.Apellido)
                        : query.OrderBy(e => e.Apellido);
                    break;

                case "email":
                    query = descending
                        ? query.OrderByDescending(e => e.Email)
                        : query.OrderBy(e => e.Email);
                    break;

                case "id":
                    query = descending
                        ? query.OrderByDescending(e => e.Id)
                        : query.OrderBy(e => e.Id);
                    break;

                default:
                    query = descending
                        ? query.OrderByDescending(e => e.Nombre)
                        : query.OrderBy(e => e.Nombre);
                    break;
            }

            var data = await query
                .Skip(Math.Max(request.Start, 0))
                .Take(request.Length > 0 ? request.Length : 10)
                .Select(e => new EstudianteTableDto
                {
                    Id = e.Id,
                    Nombre = e.Nombre ?? "",
                    Apellido = e.Apellido ?? "",
                    Email = e.Email ?? ""
                })
                .ToListAsync();

            return new DataTableResponseDto<EstudianteTableDto>
            {
                Draw = request.Draw,
                RecordsTotal = recordsTotal,
                RecordsTotalFiltered = recordsFiltered,
                Data = data
            };
        }

        public async Task<bool> UpdateAsync(Estudiante estudiante)
        {
            context.Estudiantes.Update(estudiante);
            await context.SaveChangesAsync();
            return true;
        }
    }
}