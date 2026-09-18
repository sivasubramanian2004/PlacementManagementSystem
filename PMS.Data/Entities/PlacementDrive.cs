using static System.Net.Mime.MediaTypeNames;
using PMS.Core.Helpers;
namespace PMS.Data.Entities;

public class PlacementDrive : BaseEntity
{
    public int CompanyId { get; set; }

    public string JobTitle { get; set; } = string.Empty;

    public string JobDescription { get; set; } = string.Empty;

    public EmploymentType EmploymentType { get; set; }

    public WorkMode WorkMode { get; set; }

    public string Location { get; set; } = string.Empty;

    public decimal MinimumCgpa { get; set; }

    public int MaximumBacklogs { get; set; }

    public int GraduationYear { get; set; }

    public decimal? Salary { get; set; }

    public string? RequiredSkills { get; set; }

    public DateTime ApplicationDeadline { get; set; }

    public DateTime? DriveDate { get; set; }

    public PlacementDriveStatus Status { get; set; }

    // Navigation Properties
    public Company Company { get; set; } = null!;

   public ICollection<Application> Applications { get; set; }
        = new List<Application>();

    public ICollection<PlacementDriveDepartment> PlacementDriveDepartments { get; set; }
        = new List<PlacementDriveDepartment>();
}