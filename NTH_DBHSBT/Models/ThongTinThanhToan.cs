using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NTH_DBHSBT.Models
{
    public class ThongTinThanhToan
    {
        public string maDonHang { get; set; }
        public string maHS { get; set; }
        public string maCongNo { get; set; }
        public string nganHangThanhToan { get; set; }
        public long tongTien { get; set; }
        public string ngayThanhToan { get; set; }
    }
}