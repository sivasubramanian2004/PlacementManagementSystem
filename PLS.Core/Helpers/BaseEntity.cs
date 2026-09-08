using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.Helpers
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }

        // Audit Metadata
        public DateTime CreatedDate { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int? UpdatedBy { get; set; }

        public bool IsActive { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; }

        public DateTime? DeletedDate { get; set; }

        public int? DeletedBy { get; set; }
    }
}
