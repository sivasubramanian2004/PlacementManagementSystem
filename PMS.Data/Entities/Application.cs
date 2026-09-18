using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Data.Entities
{
    public class Application : BaseEntity
    {
        public string ApplicationNumber { get; set; } = string.Empty;
        public int StudentId { get; set; }

        public int PlacementDriveId { get; set; }

        public DateTime AppliedDate { get; set; }

        public ApplicationStatus Status { get; set; }

        // Navigation Properties
        public Student Student { get; set; } = null!;

        public PlacementDrive PlacementDrive { get; set; } = null!;

      //  public ICollection<Interview> Interviews { get; set; }
       //     = new List<Interview>();
    }
}
