using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.DTOs.Auth
{
    public class ResetPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
