using PMS.Core.DTOs.Dashboard;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Service.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardResponseDto> GetDashboardAsync(
            DashboardQueryParameters request);
    }
}
