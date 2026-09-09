using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.DTOs.Auth
{
    public class  AuthRequestDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

   

}
