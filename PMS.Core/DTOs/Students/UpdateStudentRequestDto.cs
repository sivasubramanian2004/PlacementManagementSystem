using Microsoft.AspNetCore.Http;
using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.DTOs.Students
{
    public class UpdateStudentRequestDto
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public bool? IsActive { get; set; }

        public int? DepartmentId { get; set; }

        public string? RegisterNumber { get; set; }

        public Gender? Gender { get; set; }

        public string? Phone { get; set; }

        public string? Skills { get; set; }

        public PlacementStatus? PlacementStatus { get; set; }

        // Optional during PATCH
        // If provided → replace existing resume
        public IFormFile? Resume { get; set; }

        // Optional
        // If provided → replace existing profile photo
        public IFormFile? ProfilePhoto { get; set; }

        // Optional
        public List<UpdateEducationRequestDto>? Educations { get; set; }
    }
    public class UpdateEducationRequestDto
    {
        // 0 → Add new education
        // >0 → Update existing education
        public int EducationId { get; set; }

        public EducationType EducationType { get; set; }

        public string Institution { get; set; } = string.Empty;

        public decimal PercentageOrCgpa { get; set; }

        public int Backlogs { get; set; }

        public int YearOfPassing { get; set; }

        public string? Location { get; set; }
    }
}
