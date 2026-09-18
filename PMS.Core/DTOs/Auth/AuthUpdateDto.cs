using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.DTOs.Auth
{
    public class AuthUpdateDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
      
        public string? Password { get; set; }

        public UserRole? Role { get; set; }
    }
}
