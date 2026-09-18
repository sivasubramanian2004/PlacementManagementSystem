using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.DTOs.Students
{
    public class StudentResponseDto
    {
        public int StudentId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string RegisterNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public UserRole Role { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public PlacementStatus PlacementStatus { get; set; }
        public string ResumeUrl { get; set; } = string.Empty;   

    }
}
