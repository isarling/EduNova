using EduNova.DTOS;
using EduNova.Models;

namespace EduNova.Repositories
{
    public interface IEstudianteRepository
    {
        Task<DataTableResponseDto<EstudianteTableDto>> GetPagedAsync(
            DataTablesRequestDto request);

        Task<IReadOnlyList<Estudiante>> GetAllAsync();

        Task<Estudiante?> GetByIdAsync(int id);

        Task<bool> EmailExistsAsync(
            string email, int? excludeId = null);

        Task<Estudiante> CreateAsync(Estudiante estudiante);

        Task<bool> UpdateAsync(Estudiante estudiante);

        Task<bool> TieneMatriculasAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}