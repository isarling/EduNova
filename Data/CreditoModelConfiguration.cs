using EduNova.Models;
using Microsoft.EntityFrameworkCore;
namespace EduNova.Data;
public static class CreditoModelConfiguration
{
    public static void ConfigurarCredito(this ModelBuilder b)
    {
        b.Entity<Estudiante>().HasOne(e => e.Usuario).WithOne().HasForeignKey<Estudiante>(e => e.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Curso>().HasOne(c => c.CategoriaCurso).WithMany(c => c.Cursos).HasForeignKey(c => c.CategoriaCursoId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Curso>().HasOne(c => c.ModalidadCurso).WithMany(m => m.Cursos).HasForeignKey(c => c.ModalidadCursoId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<CategoriaCurso>().Property(c => c.Nombre).HasMaxLength(80);
        b.Entity<ModalidadCurso>().Property(c => c.Nombre).HasMaxLength(80);
        b.Entity<CategoriaCurso>().HasIndex(c => c.Nombre).IsUnique();
        b.Entity<ModalidadCurso>().HasIndex(c => c.Nombre).IsUnique();
        b.Entity<Matricula>().HasIndex(m => m.OperacionId).IsUnique().HasFilter("[OperacionId] IS NOT NULL");
        b.Entity<Cuota>().HasOne(c => c.DetalleMatricula).WithMany(d => d.Cuotas).HasForeignKey(c => c.DetalleMatriculaId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Cuota>().HasIndex(c => new { c.DetalleMatriculaId, c.Numero }).IsUnique();
        b.Entity<DetallePago>().HasOne(d => d.Pago).WithMany(p => p.Detalles).HasForeignKey(d => d.PagoId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<DetallePago>().HasOne(d => d.Cuota).WithMany().HasForeignKey(d => d.CuotaId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ReciboPago>().HasOne(r => r.Pago).WithOne(p => p.Recibo).HasForeignKey<ReciboPago>(r => r.PagoId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ReciboPago>().Property(r => r.NombreEstudiante).HasMaxLength(301);
        b.Entity<DetalleRecibo>().HasOne(d => d.ReciboPago).WithMany(r => r.Detalles).HasForeignKey(d => d.ReciboPagoId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<DetalleRecibo>().Property(d => d.Curso).HasMaxLength(150);
        b.Entity<BeneficioDescuento>().HasOne(b => b.Estudiante).WithMany().HasForeignKey(b => b.EstudianteId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<BeneficioDescuento>().HasOne(b => b.MatriculaOrigen).WithMany().HasForeignKey(b => b.MatriculaOrigenId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<BeneficioDescuento>().HasOne(b => b.MatriculaDestino).WithMany().HasForeignKey(b => b.MatriculaDestinoId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<BeneficioDescuento>().HasIndex(b => b.MatriculaOrigenId).IsUnique();
        b.Entity<BeneficioDescuento>().HasIndex(b => b.MatriculaDestinoId).IsUnique().HasFilter("[MatriculaDestinoId] IS NOT NULL");
        b.Entity<ConstanciaSolvencia>().HasOne(c => c.Estudiante).WithMany().HasForeignKey(c => c.EstudianteId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ConstanciaSolvencia>().Property(c => c.NombreEstudiante).HasMaxLength(301);
        b.Entity<DetalleSolvencia>().HasOne(d => d.ConstanciaSolvencia).WithMany(c => c.Detalles).HasForeignKey(d => d.ConstanciaSolvenciaId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<DetalleSolvencia>().HasOne(d => d.Matricula).WithMany().HasForeignKey(d => d.MatriculaId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<PoliticaCredito>().Property(p => p.Id).ValueGeneratedNever();
        b.Entity<Curso>().Property(c => c.DuracionDias).HasDefaultValue(90);
        b.Entity<PoliticaCredito>().ToTable(t => t.HasCheckConstraint("CK_Politica_Unica", "[Id] = 1"));
        b.Entity<Cuota>().ToTable(t => t.HasCheckConstraint("CK_Cuota_Montos", "[Monto] > 0 AND [Pagado] >= 0 AND ([Monto] - [Pagado]) >= 0"));
        b.Entity<Pago>().ToTable(t => t.HasCheckConstraint("CK_Pago_Positivo", "[Monto] > 0"));
        foreach (var entity in b.Model.GetEntityTypes())
            foreach (var property in entity.GetProperties().Where(p => p.ClrType == typeof(decimal)))
            { property.SetPrecision(12); property.SetScale(2); }
    }
}
