using EduNova.DTOS;
using EduNova.Extensions;
using EduNova.Helpers;
using EduNova.Repositories;
using EduNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduNova.Controllers
{
    [Authorize]
    public class AdminController(
        IUsuarioRepository usuarioRepository,
        IRolePermissionService rolePermissionService) : Controller
    {
        private readonly IUsuarioRepository _usuarioRepository =
            usuarioRepository;

        private readonly IRolePermissionService _rolePermissionService =
            rolePermissionService;

        private async Task<IReadOnlyList<string>>
            GetEffectivePermissionAsync()
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
            {
                return User.GetPermission();
            }

            var dbUser = await _usuarioRepository
                .GetUsuarioByIdWithPermissionAsync(userId);

            return dbUser != null
                ? EffectivePermission.FromUser(dbUser)
                : User.GetPermission();
        }

        [Authorize(Policy = "admin.panel")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Permissions =
                await GetEffectivePermissionAsync();

            return View();
        }

        [Authorize(Policy = "permiso.ver")]
        [HttpPost]
        public async Task<IActionResult> GetPermissionData(
            [FromForm] DataTablesRequestDto request)
        {
            var result = await _rolePermissionService
                .GetPermissionsPagedAsync(request);

            return Ok(result);
        }

        [Authorize(Policy = "permiso.ver")]
        [HttpGet]
        public async Task<IActionResult> Permissions()
        {
            var effective = await GetEffectivePermissionAsync();

            ViewBag.CanCreate = effective.Contains(
                "permiso.crear",
                StringComparer.OrdinalIgnoreCase);

            ViewBag.CanUpdate = effective.Any(p =>
                string.Equals(
                    p,
                    "permiso.actualizar",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    p,
                    "permiso.crear",
                    StringComparison.OrdinalIgnoreCase));

            ViewBag.CanDelete = effective.Any(p =>
                string.Equals(
                    p,
                    "permiso.borrar",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    p,
                    "permiso.crear",
                    StringComparison.OrdinalIgnoreCase));

            return View();
        }

        [Authorize(Policy = "permiso.ver")]
        [HttpGet]
        public async Task<IActionResult> GetPermissionById(int id)
        {
            var permission = await _rolePermissionService
                .GetPermissionByIdAsync(id);

            if (permission == null)
            {
                return NotFound(new
                {
                    message = "Permiso no encontrado"
                });
            }

            return Ok(permission);
        }

        [Authorize(Policy = "permiso.crear")]
        [HttpPost]
        public async Task<IActionResult> CreatePermission(
            [FromBody] SavePermissionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Revise los datos del formulario"
                });
            }

            var (success, _, errorMessage) =
                await _rolePermissionService.CreatePermissionAsync(dto);

            if (!success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = errorMessage
                });
            }

            return Ok(new
            {
                success = true,
                message = $"Permiso '{dto.Name}' creado exitosamente"
            });
        }

        [Authorize(Policy = "permiso.actualizar")]
        [HttpPost]
        public async Task<IActionResult> UpdatePermission(
            [FromBody] SavePermissionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Revise los datos del formulario"
                });
            }

            var (success, errorMessage) =
                await _rolePermissionService.UpdatePermissionAsync(dto);

            if (!success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = errorMessage
                });
            }

            return Ok(new
            {
                success = true,
                message = $"Permiso '{dto.Name}' actualizado exitosamente"
            });
        }

        [Authorize(Policy = "permiso.borrar")]
        [HttpPost]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var (success, errorMessage) =
                await _rolePermissionService.DeletePermissionAsync(id);

            if (!success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = errorMessage
                });
            }

            return Ok(new
            {
                success = true,
                message = "Permiso borrado exitosamente"
            });
        }

        [Authorize(Policy = "rol.ver")]
        [HttpGet]
        public async Task<IActionResult> Roles()
        {
            var roles = await _rolePermissionService
                .GetAllRolesAsync();

            return View(roles);
        }

        [Authorize(Policy = "rol.crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(CreateRolDto dto)
        {
            if (!ModelState.IsValid)
            {
                var roles = await _rolePermissionService
                    .GetAllRolesAsync();

                ViewBag.CreateError =
                    "Revise los datos del rol";

                return View("Roles", roles);
            }

            var (success, _, errorMessage) =
                await _rolePermissionService.CreateRoleAsync(dto);

            if (!success)
            {
                var roles = await _rolePermissionService
                    .GetAllRolesAsync();

                ViewBag.CreateError = errorMessage;

                return View("Roles", roles);
            }

            TempData["Success"] =
                $"Rol '{dto.Name}' creado exitosamente";

            return RedirectToAction(nameof(Roles));
        }

        [Authorize(Policy = "rol.asignar")]
        [HttpGet]
        public async Task<IActionResult> RoleDetails(int id)
        {
            var role = await _rolePermissionService
                .GetRoleByIdAsync(id);

            if (role == null)
                return NotFound();

            ViewBag.AllPermissions = await _rolePermissionService
                .GetAllPermissionsAsync();

            return View(role);
        }

        [Authorize(Policy = "rol.asignar")]
        [HttpPost]
        public async Task<IActionResult> AsingnPermissionToRole(
            int roleId,
            int permissionId)
        {
            // Conservamos el nombre usado por el proyecto de José.
            var (success, errorMessage) =
                await _rolePermissionService
                    .AssignPermissionToRoleAsync(roleId, permissionId);

            return Json(new
            {
                success,
                message = success
                    ? "Permiso asignado"
                    : errorMessage
            });
        }

        [Authorize(Policy = "rol.asignar")]
        [HttpPost]
        public async Task<IActionResult> RemovePermissionFromRole(
            int roleId,
            int permissionId)
        {
            var (success, errorMessage) =
                await _rolePermissionService
                    .RemovePermissionFromRoleAsync(roleId, permissionId);

            return Json(new
            {
                success,
                message = success
                    ? "Permiso removido"
                    : errorMessage
            });
        }

        [Authorize(Policy = "usuario.ver")]
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _rolePermissionService
                .GetAllUsersAsync();

            return View(users);
        }

        [Authorize(Policy = "usuario.asignar")]
        [HttpGet]
        public async Task<IActionResult> UserPermissions(int id)
        {
            var user = await _rolePermissionService
                .GetUserPermissionsAsync(id);

            if (user == null)
                return NotFound();

            ViewBag.AllPermissions = await _rolePermissionService
                .GetAllPermissionsAsync();

            ViewBag.AllRoles = await _rolePermissionService
                .GetAllRolesAsync();

            var effective = await GetEffectivePermissionAsync();

            ViewBag.CanChangeRole = effective.Contains(
                "usuario.cambiar_rol",
                StringComparer.OrdinalIgnoreCase);

            // y wwwroot/js/user-permission.js.
            return View(user);
        }

        [Authorize(Policy = "usuario.asignar")]
        [HttpPost]
        public async Task<IActionResult> AssignPermissionToUser(
            int userId,
            [FromBody] AssignUserPermission dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Revise los datos del permiso"
                });
            }

            var (success, errorMessage) =
                await _rolePermissionService
                    .AssignPermissionToUserAsync(userId, dto);

            return Json(new
            {
                success,
                message = success
                    ? (dto.IsGranted
                        ? "Permiso concedido al usuario"
                        : "Permiso denegado al usuario")
                    : errorMessage
            });
        }

        [Authorize(Policy = "usuario.asignar")]
        [HttpPost]
        public async Task<IActionResult> RemoveUserPermission(
            int userId,
            int permissionId)
        {
            var (success, errorMessage) =
                await _rolePermissionService
                    .RemoveUserPermissionAsync(userId, permissionId);

            return Json(new
            {
                success,
                message = success
                    ? "Asignación directa removida"
                    : errorMessage
            });
        }

        [Authorize(Policy = "usuario.cambiar_rol")]
        [HttpPost]
        public async Task<IActionResult> UpdateRole(
            int userId,
            [FromBody] UpdateUserRoleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Revise los datos del rol"
                });
            }

            var (success, errorMessage) =
                await _rolePermissionService
                    .UpdateUserRoleAsync(userId, dto.RoleId);

            return Json(new
            {
                success,
                message = success
                    ? "Rol actualizado"
                    : errorMessage
            });
        }
    }
}
