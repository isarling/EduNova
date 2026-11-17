using EduNova.DTOS;
using EduNova.Models;

namespace EduNova.Repositories
{
    public interface ICursoRepository
    {
        Task<bool> CategoriaValidaAsync(int? id);
        Task<bool> ModalidadValidaAsync(int? id);
        Task<DataTableResponseDto<CursoTableDto>> GetPagedAsync(DataTablesRequestDto request);
        Task<IReadOnlyList<Curso>> GetAllAsync(); 
        Task<Curso?> GetByIdAsync(int Id);
        Task<bool> NombreExisteAsync(string nombre, int? excludeId =null);
        Task<Curso> CreateAsync(Curso curso);
        Task<bool> UpdateAsync(Curso curso);

        Task<bool> DeleteAsync(int Id);
        Task<bool> HasAnyEstudentAsync(int Id);
    }
}
