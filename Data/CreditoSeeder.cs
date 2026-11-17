using EduNova.Models;
using Microsoft.EntityFrameworkCore;
namespace EduNova.Data;
public static class CreditoSeeder
{
    public static async Task SeedAsync(AppDBContext db)
    {
        if (!await db.PoliticasCredito.AnyAsync()) db.PoliticasCredito.Add(new PoliticaCredito { Id = 1 });
        var grupos = new Dictionary<string, string[]>
        {
            ["Secretaria"] = new[] { "curso.ver", "estudiante.ver", "estudiante.crear", "estudiante.actualizar", "matricula.ver", "matricula.crear", "pago.crear", "documento.ver", "solvencia.emitir" },
            ["Caja"] = new[] { "matricula.ver", "pago.crear", "documento.ver", "solvencia.emitir" },
            ["Estudiante"] = new[] { "alumno.ver" }
        };
        var nombres = grupos.Values.SelectMany(x => x).Concat(new[] { "credito.configurar", "estudiante.vincular", "catalogo.gestionar" }).Distinct().ToList();
        var admin = await db.Roles.FirstAsync(r => r.Name == "Admin");
        foreach (var nombre in nombres)
        {
            if (await db.Permissions.AnyAsync(p => p.Nombre == nombre)) continue;
            var permiso = new Permission { Nombre = nombre, Descripcion = nombre switch { "credito.configurar" => "Configurar límites de deuda y cuotas", "estudiante.vincular" => "Vincular cuentas de estudiantes", "catalogo.gestionar" => "Gestionar categorías y modalidades", "solvencia.emitir" => "Emitir constancias de solvencia", "documento.ver" => "Consultar recibos y constancias del personal", _ => "Consultar mi información de estudiante" } };
            db.Permissions.Add(permiso); await db.SaveChangesAsync();
            if (nombre != "alumno.ver") db.RolePermissions.Add(new RolePermission { RoleId = admin.Id, PermissionId = permiso.Id });
            await db.SaveChangesAsync();
        }
        foreach (var (nombre, permisos) in grupos)
        {
            if (await db.Roles.AnyAsync(r => r.Name == nombre)) continue;
            var rol = new Role { Name = nombre }; db.Roles.Add(rol); await db.SaveChangesAsync();
            foreach (var permiso in await db.Permissions.Where(p => permisos.Contains(p.Nombre)).ToListAsync()) db.RolePermissions.Add(new RolePermission { RoleId = rol.Id, PermissionId = permiso.Id });
            await db.SaveChangesAsync();
        }
        await db.SaveChangesAsync();
    }
}
