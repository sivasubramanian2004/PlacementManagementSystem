using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.DTOs.Companies
{
    public class CompanyBasicDto:CreateCompanyRequestDto
    {
        public int Id { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
