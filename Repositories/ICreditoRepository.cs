using EduNova.Models;
using Microsoft.EntityFrameworkCore.Storage;
namespace EduNova.Repositories;
public interface ICreditoRepository
{
    Task<IDbContextTransaction> BloquearEstudianteAsync(int id);
    Task<IDbContextTransaction> TransaccionAsync();
    Task<PoliticaCredito> PoliticaAsync();
    Task<List<Matricula>> MatriculasAsync(int? estudianteId = null);
    Task<Matricula?> MatriculaAsync(int id);
    Task<int?> PropietarioMatriculaAsync(int id);
    Task<Matricula?> MatriculaOperacionAsync(Guid operacion);
    Task<Pago?> PagoOperacionAsync(Guid operacion);
    Task<List<Estudiante>> EstudiantesAsync();
    Task<List<Usuario>> UsuariosAsync();
    Task<Estudiante?> EstudianteDeUsuarioAsync(int usuarioId);
    Task<List<Curso>> CursosAsync();
    Task<List<BeneficioDescuento>> BeneficiosAsync(int estudianteId);
    Task<ReciboPago?> ReciboAsync(int id);
    Task<ConstanciaSolvencia?> ConstanciaAsync(int id);
    Task<List<ConstanciaSolvencia>> ConstanciasAsync(int estudianteId);
    Task<List<CategoriaCurso>> CategoriasAsync();
    Task<List<ModalidadCurso>> ModalidadesAsync();
    void Agregar<T>(T entity) where T : class;
    void Eliminar<T>(T entity) where T : class;
    Task GuardarAsync();
}
