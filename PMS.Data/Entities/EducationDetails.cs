using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Data.Entities
{
        public class EducationDetails : BaseEntity
        {
            public int StudentId { get; set; }

            public EducationType EducationType { get; set; }

            public string Institution { get; set; } = string.Empty;

            public decimal PercentageOrCgpa { get; set; }

            public int Backlogs { get; set; }

            public int YearOfPassing { get; set; }

            public string? Location { get; set; } 

            // Navigation Property
            public Student Student { get; set; } = null!;
        }
   
}
