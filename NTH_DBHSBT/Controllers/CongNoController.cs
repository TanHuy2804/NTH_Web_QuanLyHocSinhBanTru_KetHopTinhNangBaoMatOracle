using NTH_DBHSBT.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NTH_DBHSBT.Controllers
{
    public class CongNoController : Controller
    {
        //private string connectionString = Connectring.ConnectionString;
        private string GetConnectionString()
        {
            return Session["Connecstring"] as string;
        }
        private List<CongNo> LayDanhSachCongNo()
        {
            List<CongNo> danhSachCongNo = new List<CongNo>();

            using (OracleConnection connection = new OracleConnection(GetConnectionString()))
            {
                try
                {
                    connection.Open();
                    using (OracleCommand command = new OracleCommand("NV001.CongNo_SelectAll", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("C_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CongNo congNo = new CongNo
                                {
                                    MaCongNo = reader["MaCongNo"].ToString(),
                                    MaHS = reader["MaHS"].ToString(),
                                    NgayTaoCongNo = Convert.ToDateTime(reader["NgayTaoCongNo"]),
                                    TienHocPhi = Convert.ToDecimal(reader["TienHocPhi"]),
                                    TienAn = Convert.ToDecimal(reader["TienAn"]),
                                    TienPhuThu = Convert.ToDecimal(reader["TienPhuThu"]),
                                    TongCongNo = Convert.ToDecimal(reader["TongCongNo"]),
                                    TrangThai = reader["TrangThai"].ToString()
                                };
                                danhSachCongNo.Add(congNo);
                            }

                            reader.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý exception
                    // Ví dụ: Ghi log, hiển thị thông báo lỗi, v.v.
                    Console.WriteLine(ex.Message);
                }
            }

            return danhSachCongNo;
        }

        // GET: CongNo
        public ActionResult CongNo()
        {
            List<CongNo> congNoList = LayDanhSachCongNo();
            return View(congNoList);
        }
        // GET: ThanhToanVnPay

        public string UrlPayment(int totalAmount, string orderId)
        {
            var urlPayment = "";
            TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime vietnamDateTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, vietnamTimeZone);

            string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"];
            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"];
            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"];
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];

            VnPayLibrary vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (totalAmount * 100).ToString());

            vnpay.AddRequestData("vnp_BankCode", "VNBANK");

            vnpay.AddRequestData("vnp_CreateDate", vietnamDateTime.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + orderId);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", orderId);

            urlPayment = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

            return urlPayment;
        }
        public ActionResult ThanhToanVNPAY()
        {
            string mahs = Session["Username"] as string;
            string orderid = DateTime.Now.Ticks.ToString();

            // kiểm tra tồn tại công nợ
            int trangThai = layTrangThaiCongNo(mahs);

            // lấy công nợ của học sinh
            //float total = layCongNo(mahs);
            float total = layCongNoChuaThanhToan(mahs);
            int totalInt = (int)total;

            if (trangThai == 1)
            {
                return Redirect(UrlPayment(totalInt, orderid).ToString());
                //return Content("success");
            }
            else
            {
                /*ViewBag.ErrorMessage = "Lỗi: Công nợ đã được thanh toán.";
                return View("CongNo/CongNo");*/
                return Content("Khong co cong no");
            }
        }

        public float layCongNoChuaThanhToan(string mahs)
        {
            float congno = 0;
            try
            {
                using (OracleConnection connection = new OracleConnection(GetConnectionString()))
                {
                    using (OracleCommand command = new OracleCommand("NV001.LayTongCongNoChuaThanhToan", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Thêm các tham số vào thủ tục lưu trữ
                        command.Parameters.Add("p_mahs", OracleDbType.Varchar2).Value = mahs;
                        command.Parameters.Add("p_tongconno", OracleDbType.Single).Direction = ParameterDirection.Output;

                        connection.Open();
                        command.ExecuteNonQuery();

                        // Lấy kết quả từ các tham số đầu ra
                        //object result = command.Parameters["p_tongconno"].Value;

                        // Lấy kết quả từ các tham số đầu ra
                        OracleDecimal oracleCongNo = (OracleDecimal)command.Parameters["p_tongconno"].Value;
                        congno = oracleCongNo.ToSingle();
                        Debug.WriteLine("\nTổng công nợ của học sinh: " + congno);
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có
                Debug.WriteLine("Error: " + ex.Message);
            }
            return congno;
        }

        public int capNhatTrangThaiCongNo(string mahs)
        {
            try
            {
                using (OracleConnection connection = new OracleConnection(GetConnectionString()))
                {
                    using (OracleCommand command = new OracleCommand("NV001.UpdateTrangThaiCongNo", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Thêm tham số mahs vào lệnh
                        command.Parameters.Add("p_mahs", OracleDbType.NVarchar2).Value = mahs;

                        connection.Open();
                        command.ExecuteNonQuery();

                        Debug.WriteLine("\nCập nhật trạng thái công nợ thành 'Da thanh toan' thành công.");
                        return 1; // thành công
                    }
                }
            }
            catch (OracleException ex)
            {
                Debug.WriteLine("\nLỗi cập nhật trạng thái công nợ: " + ex.Message);
                return -1; // lỗi
            }
        }

        private int layTrangThaiCongNo(string mahs)
        {
            int ketQua = -1;
            try
            {
                using (OracleConnection connection = new OracleConnection(GetConnectionString()))
                {
                    using (OracleCommand command = new OracleCommand("NV001.KiemTraTrangThaiCongNo", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Thêm các tham số vào thủ tục lưu trữ
                        command.Parameters.Add("p_mahs", OracleDbType.Varchar2).Value = mahs;

                        // Đăng ký tham số đầu ra
                        OracleParameter outputParam = new OracleParameter("p_ketqua", OracleDbType.Int32);
                        outputParam.Direction = ParameterDirection.Output;
                        command.Parameters.Add(outputParam);

                        connection.Open();
                        command.ExecuteNonQuery();

                        // Lấy kết quả từ các tham số đầu ra
                        OracleDecimal oracleCongNo = (OracleDecimal)command.Parameters["p_ketqua"].Value;
                        ketQua = oracleCongNo.ToInt32();

                        Debug.WriteLine("\nKết quả kiểm tra trạng thái công nợ: " + ketQua);
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có
                Debug.WriteLine("Error: " + ex.Message);
            }
            return ketQua;
        }

        public string layMaCongNo(string mahs)
        {
            string maCongNo = "";
            try
            {
                using (OracleConnection connection = new OracleConnection(GetConnectionString()))
                {
                    using (OracleCommand command = new OracleCommand("NV001.GetMaCongNoByMaHS", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Thêm các tham số vào thủ tục lưu trữ
                        command.Parameters.Add("p_MaHS", OracleDbType.Varchar2).Value = mahs;

                        // Đăng ký tham số đầu ra và chỉ định kiểu dữ liệu và kích thước
                        OracleParameter outputParam = new OracleParameter("p_MaCongNo", OracleDbType.Varchar2, 50);
                        outputParam.Direction = ParameterDirection.Output;
                        command.Parameters.Add(outputParam);

                        connection.Open();
                        command.ExecuteNonQuery();

                        // Lấy kết quả từ các tham số đầu ra
                        if (command.Parameters["p_MaCongNo"].Value != DBNull.Value)
                        {
                            maCongNo = command.Parameters["p_MaCongNo"].Value.ToString();
                        }
                        else
                        {
                            maCongNo = null; // Hoặc giá trị mặc định nếu không tìm thấy kết quả
                        }

                        Debug.WriteLine("\nMã công nợ được lấy ra: " + maCongNo);
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có
                Debug.WriteLine("Error: " + ex.Message);
            }
            return maCongNo;
        }

        public ActionResult XacNhanThanhToan()
        {
            var model = new ThongTinThanhToan();

            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }
                string orderCode = Convert.ToString(vnpay.GetResponseData("vnp_TxnRef")); //mã đơn hàng
                long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];
                String TerminalID = Request.QueryString["vnp_TmnCode"];
                long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100; // tổng tiền thanh toán
                String bankCode = Request.QueryString["vnp_BankCode"]; // Ngân hàng thanh toán

                model.maDonHang = orderCode;
                model.tongTien = vnp_Amount;
                model.nganHangThanhToan = bankCode;
                model.ngayThanhToan = DateTime.Now.ToString("dd/MM/yyyy"); ;

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        string mahs = Session["Username"] as string; // mã học sinh
                        string macongno = layMaCongNo(mahs); // mã công nợ

                        model.maHS = mahs;
                        model.maCongNo = macongno;

                        if (macongno == "")
                        {
                            ViewBag.KetQua = "Lấy mã công nợ thất bại...";
                        }
                        else
                        {
                            int kq = capNhatTrangThaiCongNo(mahs);
                            if (kq == 1)
                                ViewBag.KetQua = "Thanh toán thành công";
                            else
                                ViewBag.KetQua = "Thanh toán thất bại";
                        }

                    }
                    else
                        ViewBag.KetQua = "Thanh toán thất bại. Mã lỗi: " + vnp_ResponseCode;

                    ViewBag.ThanhToanThanhCong = "Số tiền thanh toán (VND):" + vnp_Amount.ToString();
                }
            }


            return View(model);
        }
    }
}