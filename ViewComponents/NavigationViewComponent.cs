using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using EduNova.Extensions;
using EduNova.Helpers;
using EduNova.Repositories;

namespace EduNova.ViewComponents
{
    public class NavigationViewComponent(
        IUsuarioRepository _userRepository) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = HttpContext.User as ClaimsPrincipal;
            var permission = user?.GetPermission() ?? [];

            if (user?.Identity?.IsAuthenticated == true &&
                int.TryParse(
                    user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    out var id))
            {
                var dbUser = await _userRepository
                    .GetUsuarioByIdWithPermissionAsync(id);

                if (dbUser != null)
                    permission = EffectivePermission.FromUser(dbUser);
            }

            return View(permission);
        }
    }
}