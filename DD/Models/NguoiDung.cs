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
    
    public partial class NguoiDung
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public NguoiDung()
        {
            this.DanhGias = new HashSet<DanhGia>();
            this.TichDiems = new HashSet<TichDiem>();
        }
    
        public string maKH { get; set; }
        public string tenKH { get; set; }
        public string email { get; set; }
        public string gioitinh { get; set; }
        public Nullable<int> thangsinh { get; set; }
        public string diachi { get; set; }
        public string sodienthoai { get; set; }
        public string tendangnhap { get; set; }
        public string matkhau { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DanhGia> DanhGias { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TichDiem> TichDiems { get; set; }
    }
}
