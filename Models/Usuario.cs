namespace EduNova.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string password { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool EmailConfirmed { get; set; }

        public bool DebeCambiarPassword { get; set; }

        public bool CuentaBloqueada { get; set; }

        public int VersionSesion { get; set; }

        public string? EmailConfirmedToken { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }

        public string? PasswordResetToken { get; set; }

        public DateTime? PasswordResetTokenExpiryTime { get; set; }

        public int? RolId { get; set; }

        public virtual Role? Rol { get; set; }

        public ICollection<UsuarioPermission> UsuarioPermissions { get; set; } = [];
    }
}