using EduNova.Models;

namespace EduNova.Services
{
    public interface ITokenService
    {

        string GenerateAccessToken(Usuario usuario);
        string GenerateRefreshToken();
        string GeneratePasswordResetToken();
    }
}
