using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.Helpers
{
    public class QueryParameters
    {
        private const int MaxPageSize = 100;

        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize
                ? MaxPageSize
                : value <= 0
                    ? 10
                    : value;
        }

        public int Skip => (PageNumber - 1) * PageSize;
        /*
         For example:

              PageNumber = 3
              PageSize   = 10
              Skip = (3 - 1) × 10 = 20 ,take=pagesize(take means fetch next records, so we use pagesize instead of take(take represent pagesize))
         */
        public string? SortBy { get; set; }

        public bool SortDescending { get; set; }

        public Dictionary<string, string> Filters { get; set; } = new();

        public Dictionary<string, List<string>> OrFilters { get; set; } = new();

        public string? SearchTerm { get; set; }
    }

    public class UserQueryParameters : QueryParameters
    {
      
        public string? Email { get; set; } 
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public UserRole? Role { get; set; }

        public bool? IsActive { get; set; } 

        public DateOnly? CreatedFrom { get; set; }
        public DateOnly? CreatedTo { get; set; }
    }

    public class EmployeeFilterRequest 
    {
        public string? EmpNo { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public int? DepartmentId { get; set; }
        public int? DesignationId { get; set; }
        public int? RoleId { get; set; }
        public string? MaritalStatus { get; set; }

        public string? Phone { get; set; }
    }
}
