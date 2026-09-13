using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.Helpers
{
    public class FileSettings
    {
        public string BasePath { get; set; } = string.Empty;
        public int MaxFileSizeInMB { get; set; }
        public List<string> AllowedExtensions { get; set; } = new();
    }
}
