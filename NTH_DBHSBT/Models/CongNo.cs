using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NTH_DBHSBT.Models
{
    public class CongNo
    {
        public string MaCongNo { get; set; }
        public string MaHS { get; set; }
        public DateTime NgayTaoCongNo { get; set; }
        public decimal TienHocPhi { get; set; }
        public decimal TienAn { get; set; }
        public decimal TienPhuThu { get; set; }
        public decimal TongCongNo { get; set; }
        public string TrangThai { get; set; }
    }
}