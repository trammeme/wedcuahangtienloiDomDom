//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DD.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class QuanTriVien
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public QuanTriVien()
        {
            this.CuaHangs = new HashSet<CuaHang>();
            this.BanTinMoiNgays = new HashSet<BanTinMoiNgay>();
        }
    
        public int adminID { get; set; }
        public string tendangnhap { get; set; }
        public string matkhau { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CuaHang> CuaHangs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BanTinMoiNgay> BanTinMoiNgays { get; set; }
    }
}
