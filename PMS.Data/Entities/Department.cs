using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Data.Entities
{
    public class Department: BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string? Manager { get; set; }

        // Navigation Property
        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
