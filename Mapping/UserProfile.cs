using AutoMapper;
using EduNova.DTOS;
using EduNova.Models;

namespace EduNova.Mapping
{
    public class UserProfile:Profile
    {
        public UserProfile()
        {
            CreateMap<Usuario, LoginResponseDto>()
               .ForMember(d => d.AccessToken, opt => opt.Ignore())
                .ForMember(d => d.RefreshToken, opt => opt.Ignore())
                 .ForMember(d => d.ExpiresAt, opt => opt.Ignore());

            CreateMap<RegisterRequestDto, Usuario>()
                  .ForMember(d => d.password, opt => opt.Ignore())
                    .ForMember(d => d.EmailConfirmed, opt => opt.Ignore())
                      .ForMember(d => d.EmailConfirmedToken, opt => opt.Ignore())
                        .ForMember(d => d.RefreshToken, opt => opt.Ignore())
                          .ForMember(d => d.RefreshTokenExpiryTime, opt => opt.Ignore())
                            .ForMember(d => d.PasswordResetToken, opt => opt.Ignore())
                              .ForMember(d => d.Rol, opt => opt.Ignore());
        }

    }
}
