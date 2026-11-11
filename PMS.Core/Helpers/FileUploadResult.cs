using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.Helpers
{
    public class FileUploadResult
    {
        public string FileName { get; set; } = string.Empty;

        //actual file name
        public string OriginalFileName { get; set; } = string.Empty;

        // Supabase bucket path; save this in PostgreSQL.
        public string RelativePath { get; set; } = string.Empty;

        //physical path on your server:
       
        public long FileSize { get; set; }
    }
}
