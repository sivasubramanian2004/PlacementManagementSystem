using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.DTOs.PlacementDrives
{
    public class UpdatePlacementDriveRequestDto
    {
        public int? CompanyId { get; set; }

        public string? JobTitle { get; set; }

        public string? JobDescription { get; set; }

        public EmploymentType? EmploymentType { get; set; }

        public WorkMode? WorkMode { get; set; }

        public string? Location { get; set; }

        public decimal? MinimumCgpa { get; set; }

        public int? MaximumBacklogs { get; set; }

        public int? GraduationYear { get; set; }

        public decimal? Salary { get; set; }

        public string? RequiredSkills { get; set; }

        public DateTime? ApplicationDeadline { get; set; }

        public DateTime? DriveDate { get; set; }

        public PlacementDriveStatus? Status { get; set; }

        public List<int>? DepartmentIds { get; set; }
    }
}
