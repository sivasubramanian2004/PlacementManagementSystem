using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.Helpers
{
    public class EmailSettings
    {
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
}
