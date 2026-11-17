using EduNova.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EduNova.Data

{
    public class AppDBContext(DbContextOptions<AppDBContext> options) : DbContext(options)
    {

        public DbSet<PoliticaCredito> PoliticasCredito => Set<PoliticaCredito>();
        public DbSet<Cuota> Cuotas => Set<Cuota>();
        public DbSet<DetallePago> DetallesPago => Set<DetallePago>();
        public DbSet<ReciboPago> RecibosPago => Set<ReciboPago>();
        public DbSet<DetalleRecibo> DetallesRecibo => Set<DetalleRecibo>();
        public DbSet<BeneficioDescuento> BeneficiosDescuento => Set<BeneficioDescuento>();
        public DbSet<ConstanciaSolvencia> ConstanciasSolvencia => Set<ConstanciaSolvencia>();
        public DbSet<DetalleSolvencia> DetallesSolvencia => Set<DetalleSolvencia>();
        public DbSet<CategoriaCurso> CategoriasCurso => Set<CategoriaCurso>();
        public DbSet<ModalidadCurso> ModalidadesCurso => Set<ModalidadCurso>();
        public DbSet<Matricula> Matriculas => Set<Matricula>();
        public DbSet<DetalleMatricula> DetallesMatricula => Set<DetalleMatricula>();
        public DbSet<Pago> Pagos => Set<Pago>();

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<UsuarioPermission> UsuarioPermissions => Set<UsuarioPermission>();
        public DbSet<Estudiante> Estudiantes => Set<Estudiante>();
        public DbSet<Curso> Cursos => Set<Curso>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigurarCredito();
            modelBuilder.Entity<Curso>().Property(c => c.Precio).HasPrecision(10, 2);
            modelBuilder.Entity<Matricula>().HasOne(m => m.Estudiante).WithMany().HasForeignKey(m => m.EstudianteId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DetalleMatricula>().HasOne(d => d.Matricula).WithMany(m => m.Detalles).HasForeignKey(d => d.MatriculaId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DetalleMatricula>().HasOne(d => d.Curso).WithMany(c => c.DetallesMatricula).HasForeignKey(d => d.CursoId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DetalleMatricula>().Property(d => d.Costo).HasPrecision(10, 2);
            modelBuilder.Entity<DetalleMatricula>().HasIndex(d => new { d.MatriculaId, d.CursoId }).IsUnique();
            modelBuilder.Entity<Pago>().HasOne(p => p.Matricula).WithMany(m => m.Pagos).HasForeignKey(p => p.MatriculaId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Pago>().Property(p => p.Monto).HasPrecision(10, 2);
            modelBuilder.Entity<Pago>().Property(p => p.Referencia).HasMaxLength(100);
            modelBuilder.Entity<Pago>().HasIndex(p => p.OperacionId).IsUnique();
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
           
            modelBuilder.Entity<UsuarioPermission>()
                .HasKey(up => new { up.UsuarioId, up.PermissionId });

            modelBuilder.Entity<UsuarioPermission>()
                .HasOne(Up => Up.Usuario)
                .WithMany(u => u.UsuarioPermissions)
                .HasForeignKey(up => up.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UsuarioPermission>()
                .HasOne(Up => Up.Permission)
                .WithMany(u => u.UsuarioPermissions)
                .HasForeignKey(up => up.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Estudiante>()
                .HasOne(e => e.Curso)
                .WithMany(c => c.Estudiantes)
                .HasForeignKey(e => e.CursoId)
                .OnDelete(DeleteBehavior.SetNull);


        }
    }
}
