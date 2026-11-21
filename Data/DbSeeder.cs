using EduNova.Models;
using Microsoft.EntityFrameworkCore;

namespace EduNova.Data;

public class DbSeeder
{
    public static async Task SeedAsync(AppDBContext context)
    {
        if (context.Database.IsSqlServer())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        var admin = await context.Roles
            .FirstOrDefaultAsync(r => r.Name == "Admin");

        var adminNuevo = admin == null;

        if (admin == null)
        {
            admin = new Role { Name = "Admin" };

            context.Roles.Add(admin);
            await context.SaveChangesAsync();
        }

        var permisos = new Dictionary<string, string>
        {
            ["estudiante.ver"] = "Consultar estudiantes",
            ["estudiante.crear"] = "Registrar estudiantes",
            ["estudiante.actualizar"] = "Editar estudiantes",
            ["estudiante.borrar"] = "Eliminar estudiantes",
            ["curso.ver"] = "Consultar cursos",
            ["curso.crear"] = "Registrar cursos",
            ["curso.actualizar"] = "Editar cursos",
            ["curso.borrar"] = "Eliminar cursos",
            ["rol.ver"] = "Consultar roles",
            ["rol.crear"] = "Crear roles",
            ["rol.asignar"] = "Administrar permisos de roles",
            ["permiso.ver"] = "Consultar permisos",
            ["permiso.crear"] = "Crear permisos",
            ["permiso.actualizar"] = "Editar permisos",
            ["permiso.borrar"] = "Eliminar permisos",
            ["usuario.ver"] = "Consultar usuarios",
            ["usuario.asignar"] = "Administrar permisos individuales",
            ["usuario.cambiar_rol"] = "Cambiar rol de un usuario",
            ["matricula.ver"] = "Consultar matrículas y saldos",
            ["matricula.crear"] = "Registrar matrículas",
            ["pago.crear"] = "Registrar abonos"
        };

        foreach (var elemento in permisos)
        {
            var permiso = await context.Permissions
                .FirstOrDefaultAsync(p => p.Nombre == elemento.Key);

            var permisoNuevo = permiso == null;

            if (permiso == null)
            {
                permiso = new Permission
                {
                    Nombre = elemento.Key,
                    Descripcion = elemento.Value
                };

                context.Permissions.Add(permiso);
                await context.SaveChangesAsync();
            }

            if (adminNuevo || permisoNuevo)
            {
                var asignado = await context.RolePermissions
                    .AnyAsync(rp =>
                        rp.RoleId == admin.Id &&
                        rp.PermissionId == permiso.Id);

                if (!asignado)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = admin.Id,
                        PermissionId = permiso.Id
                    });

                    await context.SaveChangesAsync();
                }
            }
        }

        await CreditoSeeder.SeedAsync(context);

        var existeAdministrador = await context.Usuarios
            .AnyAsync(u => u.RolId == admin.Id);

        if (!existeAdministrador)
        {
            var correoOcupado = await context.Usuarios
                .AnyAsync(u =>
                    u.Email == "admin@gmail.com" ||
                    u.UserName == "admin");

            if (correoOcupado)
            {
                throw new InvalidOperationException(
                    "La cuenta inicial admin ya existe con otro rol. " +
                    "Revisa sus permisos antes de continuar.");
            }

            context.Usuarios.Add(new Usuario
            {
                UserName = "admin",
                Email = "admin@gmail.com",
                password = BCrypt.Net.BCrypt
                    .EnhancedHashPassword("Admin123!"),
                EmailConfirmed = true,
                RolId = admin.Id
            });

            await context.SaveChangesAsync();
        }
    }
}