using Microsoft.AspNetCore.Http;
using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.DTOs.Students
{
    public class CreateStudentRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public int DepartmentId { get; set; }

        public string RegisterNumber { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public Gender Gender { get; set; }

        public string? Phone { get; set; }

        public string Skills { get; set; } = string.Empty;

        public PlacementStatus PlacementStatus { get; set; }
        public IFormFile Resume { get; set; } = null!;

        public IFormFile? ProfilePhoto { get; set; }

        public List<CreateEducationRequestDto> Educations { get; set; } = new();

    }
}
