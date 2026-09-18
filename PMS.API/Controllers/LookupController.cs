using global::PMS.Core.Helpers;
using Microsoft.AspNetCore.Mvc;
namespace PMS.API.Controllers
{
     [Route("api/[controller]")]
     [ApiController]
     public class LookupController : ControllerBase
        {
            [HttpGet("enums")]
            public IActionResult GetEnums()
            {
                return Ok(new
                {
                    PlacementStatus = GetEnumValues<PlacementStatus>(),
                    EducationType = GetEnumValues<EducationType>(),
                    Gender = GetEnumValues<Gender>(),
                    UserRole = GetEnumValues<UserRole>(),
                    IndustryType = GetEnumValues<IndustryType>(),
                    EmploymentType = GetEnumValues<EmploymentType>(),
                    WorkMode = GetEnumValues<WorkMode>(),
                    PlacementDriveStatus = GetEnumValues<PlacementDriveStatus>(),
                    ApplicationStatus = GetEnumValues<ApplicationStatus>()
                });
            }

        private static List<object> GetEnumValues<T>()
        where T : struct, Enum
        {
            return Enum.GetValues<T>()
                .Select(e => new
                {
                    Id = Convert.ToInt32(e),
                    Name = e.ToString()
                })
                .Cast<object>()
                .ToList();
        }
    }
    
}
