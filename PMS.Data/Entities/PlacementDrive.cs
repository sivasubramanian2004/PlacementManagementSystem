using static System.Net.Mime.MediaTypeNames;
using PMS.Core.Helpers;
namespace PMS.Data.Entities;

public class PlacementDrive
{
    public int Id { get; set; }

    // Company
    public int CompanyId { get; set; }


    // Job details
    public string JobTitle { get; set; } = string.Empty;

    public string JobDescription { get; set; } = string.Empty;

    public EmploymentType EmploymentType { get; set; }

    public WorkMode WorkMode { get; set; }

    public string Location { get; set; } = string.Empty;

    // Eligibility
    public decimal MinimumCGPA { get; set; }

    public int MaximumBacklogs { get; set; }

    public int GraduationYear { get; set; }

    // Compensation
    public decimal? Salary { get; set; }

    // Skills
    public string? RequiredSkills { get; set; }

    // Drive dates
    public DateTime ApplicationDeadline { get; set; }

    public DateTime? DriveDate { get; set; }

    // Status
    public PlacementDriveStatus Status { get; set; }

    public Company Company { get; set; } = null!;
    // Navigation
    //   public ICollection<Application> Applications { get; set; }
    //    = new List<Application>();
}