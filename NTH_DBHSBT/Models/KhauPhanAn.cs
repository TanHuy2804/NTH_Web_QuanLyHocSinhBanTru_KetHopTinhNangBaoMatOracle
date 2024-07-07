using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NTH_DBHSBT.Models
{
    public class KhauPhanAn
    {
        public string MaComBoMon { get; set; }
        public string LoaiComBoMon { get; set; }
        public string TenMon1 { get; set; }
        public string TenMon2 { get; set; }
        public string TenMon3 { get; set; }
        public string TenMon4 { get; set; }
        public string ThuTrongTuan { get; set; }
        public float GiaTien { get; set; }
        public string QRCode { get; set; }
    }
}