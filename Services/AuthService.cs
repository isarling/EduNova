using AutoMapper;
using EduNova.DTOS;
using EduNova.Models;
using EduNova.Models.Settings;
using EduNova.Repositories;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace EduNova.Services
{
    public class AuthService(
        IUsuarioRepository userRepo,
        IRoleRepository roleRepo,
        ITokenService tokenService,
        IEmailService emailService,
        IMapper mapper,
        IOptions<JwtSettings> jwtSettings) : IAuthService
    {
        private readonly IUsuarioRepository _userRepo = userRepo;
        private readonly IRoleRepository _roleRepo = roleRepo;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IEmailService _emailService = emailService;
        private readonly IMapper _mapper = mapper;
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;

        public async Task<bool> ChangePasswordAsync(
            string userName,
            ChangePasswordDto dto)
        {
            var validacion =
                new System.ComponentModel.DataAnnotations.ValidationContext(dto);

            var errores =
                new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            if (!System.ComponentModel.DataAnnotations.Validator
                .TryValidateObject(
                    dto,
                    validacion,
                    errores,
                    validateAllProperties: true))
            {
                return false;
            }

            var user = await _userRepo.GetUsuarioByUserName(userName);

            if (user == null ||
                user.CuentaBloqueada ||
                !_userRepo.ValidatePassword(user, dto.CurrentPassword))
            {
                return false;
            }

            if (_userRepo.ValidatePassword(user, dto.NewPassword))
            {
                return false;
            }

            user.password = BCrypt.Net.BCrypt
                .EnhancedHashPassword(dto.NewPassword);

            user.DebeCambiarPassword = false;
            user.VersionSesion++;

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiryTime = null;

            await _userRepo.SaveAsync();

            return true;
        }

        public async Task<bool> ConfirmEmailAsync(
            string email,
            string token)
        {
            var user = await _userRepo.GetUsuarioByEmail(email);

            if (user == null || user.EmailConfirmedToken != token)
            {
                return false;
            }

            var decodedToken = Uri.UnescapeDataString(token.Trim());

            if (!string.Equals(
                user.EmailConfirmedToken,
                decodedToken,
                StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            user.EmailConfirmed = true;
            user.EmailConfirmedToken = null;

            await _userRepo.SaveAsync();

            return true;
        }

        public async Task<(
            bool Success,
            LoginResponseDto? result,
            string? ErrorMessage)> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userRepo.GetUsuarioByEmail(dto.Email);

            if (user == null ||
                !_userRepo.ValidatePassword(user, dto.Password))
            {
                return (false, null, "Credenciales inválidas");
            }

            if (user.CuentaBloqueada)
            {
                return (
                    false,
                    null,
                    "Tu cuenta está bloqueada. Comunícate con la administración.");
            }

            if (!user.EmailConfirmed)
            {
                return (
                    false,
                    null,
                    "Debe confirmar su correo electrónico");
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userRepo.SaveAsync();

            var result = new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(
                    Math.Max(1, _jwtSettings.TokenValidateInMinutes)),
                RefreshTokenExpiresAt = user.RefreshTokenExpiryTime
            };

            return (true, result, null);
        }

        public async Task<(
            bool Success,
            string? ErrorMessage)> RegisterAsync(RegisterRequestDto dto)
        {
            var exist = await _userRepo.GetUsuarioByEmail(dto.Email);

            if (exist != null)
            {
                return (
                    false,
                    "Correo ya en uso. Intente con otro correo.");
            }

            var usuarioRole = await _roleRepo
                .GetByNameAsync("Estudiante");

            if (usuarioRole == null)
            {
                return (
                    false,
                    "El rol Estudiante no está configurado.");
            }

            var user = _mapper.Map<Usuario>(dto);

            user.password = BCrypt.Net.BCrypt
                .EnhancedHashPassword(dto.Password);

            user.RolId = usuarioRole.Id;

            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var rawToken = Convert.ToBase64String(tokenBytes);
            var encodedToken = Uri.EscapeDataString(rawToken);

            user.EmailConfirmedToken = rawToken;
            user.EmailConfirmed = false;

            await _userRepo.CreateAsync(user);
            await _userRepo.SaveAsync();

            string confirmationLink =
                $"https://localhost:7035/Auth/ConfirmEmail?email={user.Email}&token={encodedToken}";

            string htmlBody = $"""
                <!DOCTYPE html>
                <html>
                <body>
                    <p>Hola,</p>
                    <p>Haz clic en el siguiente botón para confirmar tu cuenta:</p>
                    <p>
                        <a href="{confirmationLink}"
                           style="background:#007BFF;color:white;padding:10px 20px;border-radius:5px;text-decoration:none;">
                            Confirmar cuenta
                        </a>
                    </p>
                </body>
                </html>
                """;

            _emailService.SendPasswordResetEmail(
                user.Email,
                htmlBody);

            return (true, null);
        }

        public async Task<bool> ResendConfirmationEmailAsync(
            string email)
        {
            var user = await _userRepo.GetUsuarioByEmail(email);

            if (user == null || user.EmailConfirmed)
            {
                return false;
            }

            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var rawToken = Convert.ToBase64String(tokenBytes);
            var encodedToken = Uri.EscapeDataString(rawToken);

            user.EmailConfirmedToken = rawToken;

            await _userRepo.SaveAsync();

            string confirmationLink =
                $"https://localhost:7035/Auth/ConfirmEmail?email={user.Email}&token={encodedToken}";

            string htmlBody = $"""
                <!DOCTYPE html>
                <html>
                <body>
                    <p>Hola,</p>
                    <p>Haz clic en el siguiente botón para confirmar tu cuenta:</p>
                    <p>
                        <a href="{confirmationLink}"
                           style="background:#007BFF;color:white;padding:10px 20px;border-radius:5px;text-decoration:none;">
                            Confirmar cuenta
                        </a>
                    </p>
                </body>
                </html>
                """;

            _emailService.SendPasswordResetEmail(
                user.Email,
                htmlBody);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(
            ResetPasswordRequestDto dto)
        {
            var user = await _userRepo.GetUsuarioByEmail(dto.Email);

            if (user == null)
            {
                return false;
            }

            var providedToken = Uri.UnescapeDataString(
                dto.Token ?? string.Empty);

            var storedToken = user.PasswordResetToken ?? string.Empty;

            if (string.IsNullOrWhiteSpace(storedToken) ||
                !storedToken.Equals(
                    providedToken,
                    StringComparison.Ordinal))
            {
                return false;
            }

            if (!user.PasswordResetTokenExpiryTime.HasValue ||
                user.PasswordResetTokenExpiryTime.Value < DateTime.UtcNow)
            {
                return false;
            }

            user.password = BCrypt.Net.BCrypt
                .EnhancedHashPassword(dto.NewPassword);

            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiryTime = null;

            user.DebeCambiarPassword = false;
            user.VersionSesion++;

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _userRepo.SaveAsync();

            return true;
        }

        public async Task<bool> SendResetPasswordLinkAsync(
            string email)
        {
            var user = await _userRepo.GetUsuarioByEmail(email);

            if (user == null)
            {
                return false;
            }

            var token = _tokenService.GeneratePasswordResetToken();
            var encodedToken = Uri.EscapeDataString(token);

            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiryTime = DateTime.UtcNow.AddHours(1);

            await _userRepo.SaveAsync();

            string confirmationLink =
                $"https://localhost:7035/Auth/ResetPassword?email={user.Email}&token={encodedToken}";

            string htmlBody = $"""
                <!DOCTYPE html>
                <html>
                <body>
                    <p>Hola,</p>
                    <p>Haz clic en el siguiente botón para recuperar tu contraseña:</p>
                    <p>
                        <a href="{confirmationLink}"
                           style="background:#007BFF;color:white;padding:10px 20px;border-radius:5px;text-decoration:none;">
                            Recuperar contraseña
                        </a>
                    </p>
                </body>
                </html>
                """;

            _emailService.SendPasswordResetEmail(
                user.Email,
                htmlBody);

            return true;
        }

        public async Task<LoginResponseDto?> RefreshAccessTokenAsync(
            string refreshToken)
        {
            var user = await _userRepo
                .GetUsuarioByRefreshTokenAsync(refreshToken);

            if (user == null ||
                user.CuentaBloqueada ||
                !user.RefreshTokenExpiryTime.HasValue ||
                user.RefreshTokenExpiryTime.Value < DateTime.UtcNow)
            {
                return null;
            }

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;

            await _userRepo.SaveAsync();

            return new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(
                    Math.Max(1, _jwtSettings.TokenValidateInMinutes)),
                RefreshTokenExpiresAt = user.RefreshTokenExpiryTime
            };
        }
    }
}