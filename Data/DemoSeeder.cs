using EduNova.Models;
using Microsoft.EntityFrameworkCore;

namespace EduNova.Data;

public static class DemoSeeder
{
    public static async Task SeedAsync(AppDBContext context)
    {
        await CrearCuentaAsync(
            context,
            "secretaria.demo",
            "secretaria.demo@example.com",
            "Secretaria123!",
            "Secretaria");

        await CrearCuentaAsync(
            context,
            "caja.demo",
            "caja.demo@example.com",
            "Caja123!",
            "Caja");

        var estudiante = await CrearCuentaAsync(
            context,
            "estudiante.demo",
            "estudiante.demo@example.com",
            "Estudiante123!",
            "Estudiante");

        var tieneRegistro = await context.Estudiantes
            .AnyAsync(e => e.UsuarioId == estudiante.Id);

        if (!tieneRegistro)
        {
            var registro = await context.Estudiantes
                .FirstOrDefaultAsync(e =>
                    e.Email == "estudiante.demo@example.com");

            if (registro == null)
            {
                context.Estudiantes.Add(new Estudiante
                {
                    Nombre = "Estudiante",
                    Apellido = "Demostración",
                    Email = "estudiante.demo@example.com",
                    UsuarioId = estudiante.Id
                });
            }
            else if (registro.UsuarioId == null)
            {
                registro.UsuarioId = estudiante.Id;
            }
            else
            {
                throw new InvalidOperationException(
                    "El registro del estudiante de demostración " +
                    "ya está asociado a otra cuenta.");
            }

            await context.SaveChangesAsync();
        }
    }

    private static async Task<Usuario> CrearCuentaAsync(
        AppDBContext context,
        string nombre,
        string email,
        string clave,
        string nombreRol)
    {
        var existente = await context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email);

        if (existente != null)
        {
            return existente;
        }

        var rol = await context.Roles
            .FirstAsync(r => r.Name == nombreRol);

        var cuenta = new Usuario
        {
            UserName = nombre,
            Email = email,
            password = BCrypt.Net.BCrypt.EnhancedHashPassword(clave),
            EmailConfirmed = true,
            RolId = rol.Id
        };

        context.Usuarios.Add(cuenta);
        await context.SaveChangesAsync();

        return cuenta;
    }
}