using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.DTOs.Auth
{
    public class UserBasicDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }


    }
}
