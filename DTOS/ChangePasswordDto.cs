using System.ComponentModel.DataAnnotations;

namespace EduNova.DTOS;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Escribe tu contraseña actual.")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe la nueva contraseña.")]
    [StringLength(
        128,
        MinimumLength = 8,
        ErrorMessage = "La contraseña debe tener entre 8 y 128 caracteres.")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9\s])\S+$",
        ErrorMessage = "Incluye mayúscula, minúscula, número y símbolo, sin espacios.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma la nueva contraseña.")]
    [Compare(
        nameof(NewPassword),
        ErrorMessage = "Las contraseñas nuevas no coinciden.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}