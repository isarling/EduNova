using EduNova.DTOS;
using EduNova.Helpers;
using EduNova.Repositories;

namespace EduNova.Services
{
    public class RolePermissionService(
        IRoleRepository roleRepo,
        IPermissionRepository permissionRepo,
        IUsuarioRepository userRepo) : IRolePermissionService
    {
        public async Task<(bool Success, string? ErrorMessage)> AssignPermissionToRoleAsync(
            int roleId, int permissionId)
        {
            var role = await roleRepo.GetByIdWithPermissionsAsync(roleId);

            if (role == null)
                return (false, "Rol no encontrado");

            var permission = await permissionRepo.GetByIdAsync(permissionId);

            if (permission == null)
                return (false, "Permiso no encontrado");

            var assigned = await roleRepo.AssingPermissionToRoleAsync(
                roleId, permissionId);

            if (!assigned)
                return (false, "El rol ya tiene asignado ese permiso");

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> AssignPermissionToUserAsync(
            int userId, AssignUserPermission dto)
        {
            var user = await userRepo.GetUsuarioByIdWithPermissionAsync(userId);

            if (user == null)
                return (false, "Usuario no encontrado");

            var permission = await permissionRepo.GetByIdAsync(dto.PermissionId);

            if (permission == null)
                return (false, "Permiso no encontrado");

            await userRepo.AssingPermissiontoUsuarioAsync(
                userId, dto.PermissionId, dto.IsGranted);

            await userRepo.SaveAsync();

            return (true, null);
        }

        public async Task<(bool Success, PermissionDto? Result, string? ErrorMessage)>
            CreatePermissionAsync(SavePermissionDto dto)
        {
            var name = dto.Name.Trim().ToLowerInvariant();

            var existing = await permissionRepo.GetByNameAsync(name);

            if (existing != null)
                return (false, null, "Ya existe un permiso con ese nombre");

            var permission = await permissionRepo.CreateAsync(
                name, dto.Descripcion);

            return (true, new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Nombre,
                Description = permission.Descripcion
            }, null);
        }

        public async Task<(bool Success, RoleWithPermissionDto? Result, string? ErrorMessage)>
            CreateRoleAsync(CreateRolDto dto)
        {
            var existing = await roleRepo.GetByNameAsync(dto.Name.Trim());

            if (existing != null)
                return (false, null, "Ya existe un rol con ese nombre");

            var role = await roleRepo.CreateAsync(dto.Name);

            return (true, new RoleWithPermissionDto
            {
                Id = role.Id,
                Name = role.Name,
                Permissions = []
            }, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeletePermissionAsync(int id)
        {
            var permission = await permissionRepo.GetByIdAsync(id);

            if (permission == null)
                return (false, "Permiso no encontrado");

            if (await permissionRepo.IsAssignedToUsuarioAsync(id))
                return (false, "No se puede eliminar el permiso asignado a uno o más usuarios");

            await permissionRepo.DeleteAsync(id);

            return (true, null);
        }

        public async Task<IReadOnlyList<PermissionDto>> GetAllPermissionsAsync()
        {
            var permissions = await permissionRepo.GetAllAsync();

            return permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Nombre,
                Description = p.Descripcion
            }).ToList();
        }

        public async Task<IReadOnlyList<RoleWithPermissionDto>> GetAllRolesAsync()
        {
            var roles = await roleRepo.GetAllWithPermissionsAsync();

            return roles.Select(MapRole).ToList();
        }

        private static RoleWithPermissionDto MapRole(
            EduNova.Models.Role role) => new()
            {
                Id = role.Id,
                Name = role.Name,
                Permissions = role.RolePermissions
                .Select(rp => new PermissionDto
                {
                    Id = rp.PermissionId,
                    Name = rp.Permission.Nombre,
                    Description = rp.Permission.Descripcion
                })
                .OrderBy(p => p.Name)
                .ToList()
            };

        public async Task<IReadOnlyList<UserListItemDto>> GetAllUsersAsync()
        {
            var users = await userRepo.GetAllUsuariosAsync();

            return users.Select(u => new UserListItemDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                RoleName = u.Rol?.Name ?? string.Empty
            }).ToList();
        }

        public async Task<PermissionDto?> GetPermissionByIdAsync(int id)
        {
            var permission = await permissionRepo.GetByIdAsync(id);

            if (permission == null)
                return null;

            return new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Nombre,
                Description = permission.Descripcion
            };
        }

        public async Task<DataTableResponseDto<PermissionDto>> GetPermissionsPagedAsync(
            DataTablesRequestDto request)
        {
            return await permissionRepo.GetPagedAsync(request);
        }

        public async Task<RoleWithPermissionDto?> GetRoleByIdAsync(int roleId)
        {
            var role = await roleRepo.GetByIdWithPermissionsAsync(roleId);

            return role == null ? null : MapRole(role);
        }

        public async Task<UserPermissionDto?> GetUserPermissionsAsync(int userId)
        {
            var user = await userRepo.GetUsuarioByIdWithPermissionAsync(userId);

            if (user == null)
                return null;

            return new UserPermissionDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                RoleId = user.RolId ?? 0,
                RoleName = user.Rol?.Name ?? string.Empty,
                EffectivePermission = EffectivePermission.FromUser(user),
                DirectPermission = user.UsuarioPermissions
                    .Where(up => up.Permission is not null)
                    .Select(up => new UserPermissionOverride
                    {
                        PermissionId = up.PermissionId,
                        PermissionName = up.Permission.Nombre,
                        IsGranted = up.IsGranted
                    })
                    .OrderBy(up => up.PermissionName)
                    .ToList()
            };
        }

        public async Task<(bool Success, string? ErrorMessage)> RemovePermissionFromRoleAsync(
            int roleId, int permissionId)
        {
            var removed = await roleRepo.RemovePermissionFromRoleAsync(
                roleId, permissionId);

            if (!removed)
                return (false, "El rol no tiene asignado este permiso");

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> RemoveUserPermissionAsync(
            int userId, int permissionId)
        {
            var removed = await userRepo.RemovePermissionFromUsuarioAsync(
                userId, permissionId);

            if (!removed)
                return (false, "El usuario no tiene una asignación directa para este permiso");

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdatePermissionAsync(
            SavePermissionDto dto)
        {
            if (!dto.Id.HasValue)
                return (false, "Id de permiso inválido");

            var permission = await permissionRepo.GetByIdAsync(dto.Id.Value);

            if (permission == null)
                return (false, "Permiso no encontrado");

            var name = dto.Name.Trim().ToLowerInvariant();
            var duplicate = await permissionRepo.GetByNameAsync(name);

            if (duplicate != null && duplicate.Id != permission.Id)
                return (false, "Ya existe un permiso con ese nombre");

            permission.Nombre = name;
            permission.Descripcion = dto.Descripcion?.Trim() ?? string.Empty;

            await permissionRepo.UpdateAsync(permission);

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateUserRoleAsync(
            int userId, int roleId)
        {
            var updated = await userRepo.UpdateUsuarioRolAsync(userId, roleId);

            if (!updated)
                return (false, "Usuario o rol no encontrado");

            await userRepo.SaveAsync();

            return (true, null);
        }
    }
}