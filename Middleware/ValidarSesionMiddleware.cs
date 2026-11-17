using System.Globalization;
using System.Security.Claims;
using EduNova.Repositories;

namespace EduNova.Middleware;

public class ValidarSesionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IUsuarioRepository usuarioRepository)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        var identificador = context.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var versionToken = context.User
            .FindFirst("version_sesion")?.Value;

        if (!int.TryParse(identificador, out var usuarioId) ||
            !int.TryParse(
                versionToken,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var version))
        {
            CerrarSesion(context);
            return;
        }

        var usuario = await usuarioRepository
            .GetUsuarioByIdWithPermissionAsync(usuarioId);

        if (usuario == null ||
            usuario.CuentaBloqueada ||
            usuario.VersionSesion != version)
        {
            CerrarSesion(context);
            return;
        }

        if (usuario.DebeCambiarPassword)
        {
            var controller = context.Request.RouteValues["controller"]?
                .ToString();

            var action = context.Request.RouteValues["action"]?
                .ToString();

            var esAuth = string.Equals(
                controller,
                "Auth",
                StringComparison.OrdinalIgnoreCase);

            var puedeContinuar = esAuth &&
                (
                    string.Equals(
                        action,
                        "ChangePassword",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    string.Equals(
                        action,
                        "Logout",
                        StringComparison.OrdinalIgnoreCase)
                );

            if (!puedeContinuar)
            {
                context.Response.Redirect(
                    context.Request.PathBase + "/Auth/ChangePassword");

                return;
            }
        }

        await next(context);
    }

    private static void CerrarSesion(HttpContext context)
    {
        var opciones = new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

        context.Response.Cookies.Delete("access_token", opciones);
        context.Response.Cookies.Delete("refresh_token", opciones);

        context.Response.Redirect(
            context.Request.PathBase + "/Auth/Login");
    }
}