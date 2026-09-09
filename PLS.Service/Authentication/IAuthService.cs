using PMS.Core.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Service.Authentication
{
    public interface IAuthService

    {
        Task<AuthResponseDto> RegisterAsync(AuthRequestDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task UpdateAsync(int id, AuthUpdateDto dto);

        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task ResetPasswordAsync(ResetPasswordDto dto);
    }
}
