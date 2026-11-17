using EduNova.Models;

namespace EduNova.Helpers
{
    public static class EffectivePermission
    {
        public static List<string> FromUser(Usuario user)
        {
            var permissions = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            if (user.Rol is not null)
            {
                foreach (var permission in user.Rol.RolePermissions
                    .Select(rp => rp.Permission?.Nombre)
                    .Where(name => !string.IsNullOrWhiteSpace(name)))
                {
                    permissions.Add(permission!);
                }
            }

            foreach (var overridePermission in user.UsuarioPermissions)
            {
                var name = overridePermission.Permission?.Nombre;

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (overridePermission.IsGranted)
                    permissions.Add(name);
                else
                    permissions.Remove(name);
            }

            return permissions
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}