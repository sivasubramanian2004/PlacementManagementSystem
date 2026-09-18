using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.DTOs.Applications
{
    public class ApplicationResponseDto
    {
        public int ApplicationId { get; set; }

        public string ApplicationNumber { get; set; } = string.Empty;

        public int StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string RegisterNumber { get; set; }=string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public int PlacementDriveId { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public int CompanyId { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public DateTime AppliedDate { get; set; }

        public ApplicationStatus ApplicationStatus { get; set; }
    }
}
