using EduNova.DTOS;
using EduNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduNova.Controllers
{
    public class AuthController(IAuthService authService) : Controller
    {
        private readonly IAuthService _authService = authService;

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Datos de registro inválidos"
                });
            }

            var (success, error) =
                await _authService.RegisterAsync(dto);

            if (!success)
            {
                return Conflict(new { message = error });
            }

            return Ok(new { success = true });
        }

        [HttpGet]
        public IActionResult Login()
        {
            var token = Request.Cookies["access_token"];

            if (!string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    errorMessage = "Datos inválidos"
                });
            }

            var (success, result, error) =
                await _authService.LoginAsync(dto);

            if (!success || result is null)
            {
                return Unauthorized(new
                {
                    success = false,
                    errorMessage = error ?? "Credenciales inválidas"
                });
            }

            var accessOptions = new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.ExpiresAt
            };

            var refreshOptions = new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.RefreshTokenExpiresAt
            };

            Response.Cookies.Append(
                "access_token",
                result.AccessToken,
                accessOptions);

            Response.Cookies.Append(
                "refresh_token",
                result.RefreshToken,
                refreshOptions);

            return Ok(new
            {
                success = true,
                message = "Login exitoso"
            });
        }

        [Authorize]
        [HttpPost]
        public IActionResult Logout()
        {
            var opciones = new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            Response.Cookies.Delete("access_token", opciones);
            Response.Cookies.Delete("refresh_token", opciones);

            return Json(new
            {
                success = true,
                message = "La sesión ha sido cerrada exitosamente"
            });
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(
            string email,
            string token)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Parámetros inválidos");
            }

            var confirmado =
                await _authService.ConfirmEmailAsync(email, token);

            if (confirmado)
            {
                TempData["Success"] =
                    "Correo confirmado, ya puedes iniciar sesión";

                return RedirectToAction("Login");
            }

            TempData["Error"] = "El token es inválido o expiró";

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgetPasswordDto model)
        {
            if (model == null ||
                !ModelState.IsValid ||
                string.IsNullOrWhiteSpace(model.Email))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Ingresa un correo válido",
                    errorMessage = "Ingresa un correo válido"
                });
            }

            var sent = await _authService
                .SendResetPasswordLinkAsync(model.Email);

            if (sent)
            {
                return Ok(new { success = true });
            }

            return NotFound(new
            {
                success = false,
                message = "Correo no encontrado",
                errorMessage = "Correo no encontrado"
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(
            string email,
            string token)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Parámetros inválidos");
            }

            var model = new ResetPasswordRequestDto
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordRequestDto dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Datos inválidos para restablecer la contraseña",
                    errorMessage =
                        "Datos inválidos para restablecer la contraseña"
                });
            }

            var success =
                await _authService.ResetPasswordAsync(dto);

            if (success)
            {
                return Ok(new { success = true });
            }

            return BadRequest(new
            {
                success = false,
                message = "Token inválido o expirado",
                errorMessage = "Token inválido o expirado"
            });
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordDto());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var userName = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(userName))
            {
                return Unauthorized();
            }

            var success = await _authService
                .ChangePasswordAsync(userName, dto);

            if (!success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo cambiar la contraseña. Comprueba la clave " +
                    "actual y utiliza una nueva diferente.");

                return View(dto);
            }

            var opciones = new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            Response.Cookies.Delete("access_token", opciones);
            Response.Cookies.Delete("refresh_token", opciones);

            TempData["Success"] =
                "Contraseña actualizada. Inicia sesión con tu nueva contraseña.";

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        [HttpPost]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken =
                Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized();
            }

            var newToken = await _authService
                .RefreshAccessTokenAsync(refreshToken);

            if (newToken == null)
            {
                return Unauthorized();
            }

            var accessOptions = new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = newToken.ExpiresAt
            };

            var refreshOptions = new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = newToken.RefreshTokenExpiresAt
            };

            Response.Cookies.Append(
                "access_token",
                newToken.AccessToken,
                accessOptions);

            Response.Cookies.Append(
                "refresh_token",
                newToken.RefreshToken,
                refreshOptions);

            return Ok(new { success = true });
        }
    }
}