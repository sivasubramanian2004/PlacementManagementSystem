using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.DTOs.PlacementDrives
{
    public class PlacementDriveResponseDto {

        public int PlacementDriveId { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string JobTitle { get; set; } = string.Empty;

        public string JobDescription { get; set; } = string.Empty;

        public EmploymentType EmploymentType { get; set; }

        public WorkMode WorkMode { get; set; }

        public DateOnly ApplicationDeadline { get; set; }
        public PlacementDriveStatus DriveStatus { get; set; }

        public DateOnly? DriveDate { get; set; }

    }
    public class PlacementDriveDto
    {
        public int PlacementDriveId { get; set; }

        public string CompanyName { get; set; }=string.Empty;

        public string JobTitle { get; set; } = string.Empty;

        public string JobDescription { get; set; } = string.Empty;

        public EmploymentType EmploymentType { get; set; }

        public WorkMode WorkMode { get; set; }

        public string Location { get; set; } = string.Empty;

        public decimal MinimumCgpa { get; set; }

        public int MaximumBacklogs { get; set; }

        public int GraduationYear { get; set; }

        public decimal? Salary { get; set; }

        public string? RequiredSkills { get; set; }

        public DateOnly ApplicationDeadline { get; set; }

        public DateOnly? DriveDate { get; set; }

        public PlacementDriveStatus DriveStatus { get; set; }

        public List<string> DepartmentNames { get; set; } = [];
    }
}
