using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Data.Entities
{
    public class Company:BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public IndustryType IndustryType { get; set; }

        public string? OtherIndustry { get; set; }

        public string? Website { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Description { get; set; }

        public string? Location { get; set; }

        public ICollection<PlacementDrive> PlacementDrives { get; set; } = new List<PlacementDrive>();

    }
}
