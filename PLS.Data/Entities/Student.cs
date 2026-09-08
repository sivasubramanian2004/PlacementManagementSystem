using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace PMS.Data.Entities
{
    public class Student:BaseEntity
    {
        public int UserId { get; set; }

        public int DepartmentId { get; set; }

        public string RegisterNumber { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public Gender Gender { get; set; }

        public string? Phone { get; set; }

        public string Skills { get; set; } = string.Empty;

        public PlacementStatus PlacementStatus { get; set; } = PlacementStatus.NotPlaced;


        public string ResumeUrl { get; set; } = string.Empty;

        public string? ProfilePictureUrl { get; set; }

        // Navigation Properties
        public Department Department { get; set; } = null!;

        public User User { get; set; } = null!;

        //  public ICollection<Application> Applications { get; set; } = new List<Application>();

        public ICollection<EducationDetails> Educations { get; set; } = new List<EducationDetails>();
    }
}
