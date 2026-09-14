using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.DTOs.Students
{
    public class StudentBasicDto
    {
        public int StudentId { get; set; }

        public int UserId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string RegisterNumber { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public Gender Gender { get; set; }

        public string? Phone { get; set; }

        public string Status { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }

        public string ResumeUrl { get; set; } = string.Empty;

        public PlacementStatus PlacementStatus { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public List<EducationDto> Educations { get; set; } = new();
    }
}
