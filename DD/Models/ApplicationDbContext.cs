using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace DD.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("name=DomDomStore") // Tên chuỗi kết nối
        {
        }
        public DbSet<QLUserViewModel> QLUsers { get; set; }

        // Thêm DbSet cho các thực thể khác nếu cần
    }
}