using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.DTOs.Dashboard
{
    public class DashboardQueryParameters
    {
        public Gender? Gender { get; set; }

        public PlacementStatus? PlacementStatus { get; set; }
    }
}
