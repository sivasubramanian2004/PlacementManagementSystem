using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.DTOs.Companies
{
    public class CompanyResponseDto
    {
        public string Name { get; set; } = string.Empty;

        public IndustryType IndustryType { get; set; }

        public string? Email { get; set; }
    }
}
