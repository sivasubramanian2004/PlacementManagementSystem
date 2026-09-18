using PMS.Core.DTOs.Students;
using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.DTOs.Applications
{
    public class ApplicationDto
    {
        public int ApplicationId { get; set; }

        public string ApplicationNumber { get; set; } = string.Empty;

        // Student
        public int StudentId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string RegisterNumber { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;
        // Placement Drive
        public int PlacementDriveId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string? Location { get; set; } = string.Empty;

        // Company details

        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public IndustryType IndustryType { get; set; }
        public string? OtherIndustry { get; set; }
        public string? Website { get; set; }
        public string? CompanyEmail { get; set; }
        public string? CompanyPhone { get; set; }
        public string? CompanyDescription { get; set; }

        // Application
        public DateTime AppliedDate { get; set; }
        public ApplicationStatus ApplicationStatus { get; set; }
    }

    public class DownloadApplication: ApplicationDto
    {

        public int EducationId { get; set; }

        public EducationType EducationType { get; set; }

        public string Institution { get; set; } = string.Empty;

        public decimal PercentageOrCgpa { get; set; }

        public int Backlogs { get; set; }

        public int YearOfPassing { get; set; }






    }


}
