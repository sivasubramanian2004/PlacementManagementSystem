using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.DTOs.Auth
{
    public class LoginRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto:UserBasicDto
    {
       
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
       
    }

}
