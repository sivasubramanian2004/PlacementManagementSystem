using PMS.Core.DTOs.Auth;
using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Service.Authentication
{
    public interface IAuthService

    {
        Task<AuthResponseDto> RegisterAsync(AuthRequestDto dto);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto  dto);
        Task UpdateAsync(int id, AuthUpdateDto dto);

        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task ResetPasswordAsync(ResetPasswordDto dto);

        Task<PagedResult<UserBasicDto>> GetAllUsersAsync(UserQueryParameters request);
    }
}
