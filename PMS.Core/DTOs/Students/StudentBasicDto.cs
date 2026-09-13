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

        public string Name { get; set; } = string.Empty;

        public Gender Gender { get; set; }

        public string? Phone { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public PlacementStatus PlacementStatus { get; set; }

        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;
    }
}
