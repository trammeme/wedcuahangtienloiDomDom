using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DD.Models
{
    public class AuditLog
    {

        public int Id { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public string PerformedBy { get; set; }
        public int? UserId { get; set; } // ✅ Thêm dòng này

        public DateTime PerformedAt { get; set; }
    }
}