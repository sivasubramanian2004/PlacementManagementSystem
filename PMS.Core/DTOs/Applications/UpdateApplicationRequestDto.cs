using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.DTOs.Applications
{
    public class UpdateApplicationRequestDto
    {
        public ApplicationStatus? ApplicationStatus { get; set; }

        public bool? IsActive { get; set; }
    }
}
