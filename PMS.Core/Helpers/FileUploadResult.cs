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

        //relative path for db table store
        public string RelativePath { get; set; } = string.Empty;

        //physical path on your server:
        public string FullPath { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }
}
