using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.DTOs.Dashboard
{
    public class DashboardResponseDto
    {
        // Students
        public int TotalStudents { get; set; }
        public int PlacedStudents { get; set; }
        public int UnplacedStudents { get; set; }

        // Applications
        public int TotalApplications { get; set; }
        public int AppliedApplications { get; set; }
        public int ShortlistedApplications { get; set; }
        public int SelectedApplications { get; set; }

        // Master Data
        public int TotalDepartments { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalPlacementDrives { get; set; }

        // Gender
        public List<GenderCountDto> GenderWiseCount { get; set; } = [];
    }
}
