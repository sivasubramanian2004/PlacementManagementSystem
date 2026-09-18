using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.DTOs.Companies
{
    public class CreateCompanyRequestDto
    {
        public string Name { get; set; } = string.Empty;

        public IndustryType IndustryType { get; set; }

        public string? OtherIndustry { get; set; }

        public string? Website { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Description { get; set; }

        public string? Location { get; set; }
    }
}
