using EduNova.Data;
using EduNova.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
namespace EduNova.Repositories;
public class CreditoRepository(AppDBContext db) : ICreditoRepository
{
    public Task<IDbContextTransaction> TransaccionAsync() => db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
    public async Task<IDbContextTransaction> BloquearEstudianteAsync(int id)
    {
        var tx = await TransaccionAsync();
        try
        {
            // Un mismo bloqueo protege matrículas, pagos y documentos del estudiante.
            if (db.Database.IsSqlServer()) await db.Estudiantes.FromSqlInterpolated($"SELECT * FROM [Estudiantes] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {id}").ToListAsync();
            return tx;
        }
        catch { await tx.DisposeAsync(); throw; }
    }
    private IQueryable<Matricula> Consulta() => db.Matriculas.AsSplitQuery()
        .Include(m => m.Estudiante)
        .Include(m => m.Detalles).ThenInclude(d => d.Curso)
        .Include(m => m.Detalles).ThenInclude(d => d.Cuotas)
        .Include(m => m.Pagos).ThenInclude(p => p.Recibo);
    public Task<int?> PropietarioMatriculaAsync(int id) => db.Matriculas.AsNoTracking().Where(m => m.Id == id).Select(m => (int?)m.EstudianteId).SingleOrDefaultAsync();
    public Task<Matricula?> MatriculaOperacionAsync(Guid operacion) => db.Matriculas.AsNoTracking().SingleOrDefaultAsync(m => m.OperacionId == operacion);
    public Task<Pago?> PagoOperacionAsync(Guid operacion) => db.Pagos.AsNoTracking().Include(p => p.Recibo).SingleOrDefaultAsync(p => p.OperacionId == operacion);
    public Task<PoliticaCredito> PoliticaAsync() => db.PoliticasCredito.SingleAsync(p => p.Id == 1);
    public Task<List<Matricula>> MatriculasAsync(int? estudianteId = null) => Consulta().Where(m => !estudianteId.HasValue || m.EstudianteId == estudianteId.Value).OrderByDescending(m => m.Id).ToListAsync();
    public Task<Matricula?> MatriculaAsync(int id) => Consulta().SingleOrDefaultAsync(m => m.Id == id);
    public Task<List<Estudiante>> EstudiantesAsync() => db.Estudiantes.OrderBy(e => e.Nombre).ToListAsync();
    public Task<List<Usuario>> UsuariosAsync() => db.Usuarios.Include(u => u.Rol).OrderBy(u => u.UserName).ToListAsync();
    public Task<Estudiante?> EstudianteDeUsuarioAsync(int usuarioId) => db.Estudiantes.SingleOrDefaultAsync(e => e.UsuarioId == usuarioId);
    public Task<List<Curso>> CursosAsync() => db.Cursos.OrderBy(c => c.Nombre).ToListAsync();
    public Task<List<BeneficioDescuento>> BeneficiosAsync(int estudianteId) => db.BeneficiosDescuento.Where(b => b.EstudianteId == estudianteId).OrderBy(b => b.Vence).ToListAsync();
    public Task<ReciboPago?> ReciboAsync(int id) => db.RecibosPago.Include(r => r.Detalles).Include(r => r.Pago).ThenInclude(p => p.Matricula).ThenInclude(m => m.Estudiante).SingleOrDefaultAsync(r => r.Id == id);
    public Task<ConstanciaSolvencia?> ConstanciaAsync(int id) => db.ConstanciasSolvencia.Include(c => c.Detalles).Include(c => c.Estudiante).SingleOrDefaultAsync(c => c.Id == id);
    public Task<List<ConstanciaSolvencia>> ConstanciasAsync(int estudianteId) => db.ConstanciasSolvencia.Where(c => c.EstudianteId == estudianteId).OrderByDescending(c => c.Id).ToListAsync();
    public Task<List<CategoriaCurso>> CategoriasAsync() => db.CategoriasCurso.Include(c => c.Cursos).ToListAsync();
    public Task<List<ModalidadCurso>> ModalidadesAsync() => db.ModalidadesCurso.Include(m => m.Cursos).ToListAsync();
    public void Agregar<T>(T entity) where T : class => db.Add(entity);
    public void Eliminar<T>(T entity) where T : class => db.Remove(entity);
    public Task GuardarAsync() => db.SaveChangesAsync();
}
