using Microsoft.EntityFrameworkCore;
using EduNova.Models;

namespace EduNova.Data
{
    public class DbSeeder
    {
        public static async Task SeedAsync(AppDBContext context)
        {
            if (context.Database.IsSqlServer()) await context.Database.MigrateAsync();
            else await context.Database.EnsureCreatedAsync();

            await SeedRolesAsync(context);
            await SeedPermissionsAsync(context);
            await SeedRolePermissionsAsync(context);
            await SeedMatriculaPermissionsAsync(context);
            await SeedUsersAsync(context);
            await CreditoSeeder.SeedAsync(context);
        }

        private static async Task SeedMatriculaPermissionsAsync(AppDBContext context)
        {
            var nombres = new[] { "matricula.ver", "matricula.crear", "pago.crear" };
            var admin = await context.Roles.FirstAsync(r => r.Name == "Admin");
            foreach (var nombre in nombres)
            {
                var permiso = await context.Permissions.FirstOrDefaultAsync(p => p.Nombre == nombre);
                if (permiso == null)
                {
                    permiso = new Permission { Nombre = nombre, Descripcion = nombre == "pago.crear" ? "Registrar abonos" : nombre == "matricula.ver" ? "Consultar matrículas y saldos" : "Crear matrículas" };
                    context.Permissions.Add(permiso);
                    await context.SaveChangesAsync();
                    context.RolePermissions.Add(new RolePermission { RoleId = admin.Id, PermissionId = permiso.Id });
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task SeedRolesAsync(AppDBContext context)
        {
            var changed = false;
            if (!await context.Roles.AnyAsync(r => r.Name == "Admin"))
            {
                context.Roles.Add(new Role { Name = "Admin" });
                changed = true;
            }
            if (!await context.Roles.AnyAsync(r => r.Name == "Usuario"))
            {
                context.Roles.Add(new Role { Name = "Usuario" });
                changed = true;
            }
            if (changed)
            {
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedPermissionsAsync(AppDBContext context)
        {
            if (await context.Permissions.AnyAsync())
                return;
            context.Permissions.AddRange(
                new Permission { Nombre = "estudiante.ver", Descripcion = "Ver estudiante" },
                new Permission { Nombre = "estudiante.crear", Descripcion = "Crear estudiante" },
                new Permission { Nombre = "estudiante.actualizar", Descripcion = "Actualizar estudiante" },
                new Permission { Nombre = "estudiante.borrar", Descripcion = "Borrar estudiante" },
                new Permission { Nombre = "rol.ver", Descripcion = "Ver roles" },
                new Permission { Nombre = "rol.crear", Descripcion = "Crear rol" },
                new Permission { Nombre = "rol.asignar", Descripcion = "Asignar rol" },
                new Permission { Nombre = "permiso.ver", Descripcion = "Ver permisos" },
                new Permission { Nombre = "permiso.crear", Descripcion = " Crear permisos" },
                new Permission { Nombre = "permiso.actualizar", Descripcion = "Actualizar permisos" },
                new Permission { Nombre = "permiso.borrar", Descripcion = "Borrar permisos" },
                new Permission { Nombre = "usuario.ver", Descripcion = "Ver usuarios" },
                new Permission { Nombre = "usuario.asignar", Descripcion = "Asignar usuarios" },
                new Permission { Nombre = "usuario.cambiar_rol", Descripcion = "Cambiar rol de usuario" },
                new Permission { Nombre = "curso.ver", Descripcion = "Ver curso" },
                new Permission { Nombre = "curso.crear", Descripcion = "Crear curso" },
                new Permission { Nombre = "curso.actualizar", Descripcion = "Actualizar curso" },
                new Permission { Nombre = "curso.borrar", Descripcion = "Borrar curso " });
            await context.SaveChangesAsync();
        }

        private static async Task SeedRolePermissionsAsync(AppDBContext context)
        {
            if (await context.RolePermissions.AnyAsync())
                return;
            var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
            var usuarioRole = await context.Roles.FirstAsync(r => r.Name == "Usuario");

            var permissions = await context.Permissions.ToListAsync();

            foreach (var permission in permissions)
            {
                context.RolePermissions.Add(new RolePermission
                {

                    RoleId = adminRole.Id,
                    PermissionId = permission.Id
                });
            }

            var usuarioPermissions = permissions.
                Where(p => p.Nombre is "estudiante.ver" or "curso.ver")
                .ToList();

            foreach (var permission in usuarioPermissions)
            {

                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = usuarioRole.Id,
                    PermissionId = permission.Id
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedUsersAsync(AppDBContext context)
        {
            if (await context.Usuarios.AnyAsync(u => u.UserName == "admin"))
            {
                return;
            }

            var adminRole = await context.Roles
                .FirstAsync(r => r.Name == "Admin");

            context.Usuarios.Add(new Usuario
            {
                UserName = "admin",
                Email = "admin@gmail.com",
                password = BCrypt.Net.BCrypt.EnhancedHashPassword("Admin123!"),
                EmailConfirmed = true,
                RolId = adminRole.Id
            });

            await context.SaveChangesAsync();
        }

    }
}
