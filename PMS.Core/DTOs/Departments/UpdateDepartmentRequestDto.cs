using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.DTOs.Departments
{
    public class UpdateDepartmentRequestDto
    {
        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
