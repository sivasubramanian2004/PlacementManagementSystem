using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Data.Entities
{
    public class PlacementDriveDepartment : BaseEntity
    {
        public int PlacementDriveId { get; set; }

        public int DepartmentId { get; set; }

        // Navigation Properties
        public PlacementDrive PlacementDrive { get; set; } = null!;

        public Department Department { get; set; } = null!;
    }
}
