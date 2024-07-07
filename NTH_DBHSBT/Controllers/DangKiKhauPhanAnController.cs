using NTH_DBHSBT.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NTH_DBHSBT.Controllers
{
    public class DangKiKhauPhanAnController : Controller
    {
        //string connectionString = Connectring.ConnectionString;
        private string GetConnectionString()
        {
            return Session["Connecstring"] as string;
        }
        // hiển thị thong tin các món ăn 
        public ActionResult HienThiKPA()
        {
            List<KhauPhanAn> comBoMonList = new List<KhauPhanAn>();
            Debug.Write("Connectring: " + GetConnectionString());

            using (OracleConnection connection = new OracleConnection(GetConnectionString()))
            {
                OracleCommand command = new OracleCommand("NV001.GetComboMonan", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("comboMonan", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                try
                {
                    connection.Open();
                    OracleDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        KhauPhanAn comBoMon = new KhauPhanAn
                        {
                            MaComBoMon = reader["macombomon"].ToString(),
                            LoaiComBoMon = reader["loaicombomon"].ToString(),
                            TenMon1 = reader["tenmon1"].ToString(),
                            TenMon2 = reader["tenmon2"].ToString(),
                            TenMon3 = reader["tenmon3"].ToString(),
                            TenMon4 = reader["tenmon4"].ToString(),
                            ThuTrongTuan = reader["thutrongtuan"].ToString(),
                            GiaTien = Convert.ToInt32(reader["giatien"]),
                            QRCode = reader["qrcode"].ToString(),
                        };
                        comBoMonList.Add(comBoMon);
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex.Message);
                }
            }

            if (comBoMonList.Count > 0)
            {
                KhauPhanAn firstComboMon = comBoMonList[0];
                Debug.WriteLine($"Mã Combo Món: {firstComboMon.MaComBoMon}, Loại Combo Món: {firstComboMon.LoaiComBoMon}, Tên Món 1: {firstComboMon.TenMon1}, ...");
            }
            else
            {
                Debug.WriteLine("Danh sách Combo Món trống.");
            }

            return View("IndexKAP", comBoMonList);
        }

        public ActionResult IndexKAP()
        {
            return View();
        }

        public int LayThongTinDangKi(string macombo, out int thuTrongTuan, out float giatien, out string qrCode)
        {
            // Khởi tạo các giá trị đầu ra mặc định
            thuTrongTuan = 0;
            giatien = 0;
            qrCode = string.Empty;

            // Kiểm tra xem macombo có null hoặc rỗng không
            if (string.IsNullOrEmpty(macombo))
            {
                return -1; // lỗi
            }

            try
            {
                using (OracleConnection connection = new OracleConnection(GetConnectionString()))
                {
                    using (OracleCommand command = new OracleCommand("NV001.LayThongTinDangKiCur", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Thêm các tham số vào thủ tục lưu trữ
                        command.Parameters.Add("p_macombo", OracleDbType.Varchar2).Value = macombo;
                        command.Parameters.Add("p_thutrongtuan", OracleDbType.Int32).Direction = ParameterDirection.Output;
                        command.Parameters.Add("p_giatien", OracleDbType.Single).Direction = ParameterDirection.Output;
                        command.Parameters.Add("p_qrcode", OracleDbType.Varchar2, 100).Direction = ParameterDirection.Output;

                        connection.Open();
                        command.ExecuteNonQuery();

                        // Lấy kết quả từ các tham số đầu ra
                        qrCode = command.Parameters["p_qrcode"].Value.ToString();
                        OracleDecimal oracleGiatien = (OracleDecimal)command.Parameters["p_giatien"].Value;
                        giatien = (float)oracleGiatien.ToDouble();
                        thuTrongTuan = ((OracleDecimal)command.Parameters["p_thutrongtuan"].Value).ToInt32();


                        Debug.WriteLine("\nQR code lấy thông tin đăng kí: " + qrCode);

                        return 1; // thành công
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có
                Debug.WriteLine("Error: " + ex.Message);
                return -1; // lỗi
            }
        }

        public int InsertDangKyKhauPhanAn(string p_MaHS, string p_NgayDangKyKP, int p_ThuTrongTuan, float p_Gia, string p_MaComBoMon, string p_QRCode)
        {
            using (OracleConnection connection = new OracleConnection(GetConnectionString()))
            {
                try
                {
                    OracleCommand command = new OracleCommand("NV001.InsertDangKyKhauPhanAn", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    // Thêm các tham số vào thủ tục
                    command.Parameters.Add("p_MaHS", OracleDbType.Varchar2).Value = p_MaHS;
                    command.Parameters.Add("p_NgayDangKyKP", OracleDbType.Varchar2).Value = p_NgayDangKyKP;
                    command.Parameters.Add("p_ThuTrongTuan", OracleDbType.Int32).Value = p_ThuTrongTuan;
                    command.Parameters.Add("p_Gia", OracleDbType.Single).Value = p_Gia;
                    command.Parameters.Add("p_MaComBoMon", OracleDbType.Varchar2).Value = p_MaComBoMon;
                    command.Parameters.Add("p_QRCode", OracleDbType.Varchar2).Value = p_QRCode;
                    command.Parameters.Add("p_MaNV", OracleDbType.Varchar2).Value = p_MaHS;
                    connection.Open();
                    command.ExecuteNonQuery();

                    Debug.WriteLine("\nDữ liệu đã được thêm thành công.");
                    return 1; // thành công
                }
                catch (OracleException ex)
                {
                    Debug.WriteLine("\nLỗi khi thêm dữ liệu: " + ex.Message);
                    return -1; // lỗi
                }
            }
        }

        public ActionResult DangKi(string macombo)
        {
            int thuTrongTuan;
            float giatien;
            string qrCode;

            // Gọi hàm LayThongTinDangKi với tham số macombo
            int kq = LayThongTinDangKi(macombo, out thuTrongTuan, out giatien, out qrCode);
            if (kq == 1)
            {
                string mahs = Session["Username"] as string;
                // Lấy ngày và thời gian hệ thống
                DateTime currentDate = DateTime.Now;
                // Chuyển đổi thành chuỗi
                string dateString = currentDate.ToString("yyyy-MM-dd");
                Debug.WriteLine("\nUser thực hiện đăng kí: " + mahs);
                Debug.WriteLine("\nNgày đăng kí: " + dateString);
                Debug.WriteLine("\nThứ trong tuần: " + thuTrongTuan);
                Debug.WriteLine("\nGiá tiền: " + giatien);
                Debug.WriteLine("\nMã combo: " + macombo);
                Debug.WriteLine("\nQR code: " + qrCode);
                int kqInsert = InsertDangKyKhauPhanAn(mahs, dateString, thuTrongTuan, giatien, macombo, qrCode);
                Debug.WriteLine("\nKết quả insert: " + kqInsert);
                if (kqInsert == 1)
                {
                    return Content("success");
                }
                else
                {
                    return Content("error");
                }
            }
            else
            {
                Debug.WriteLine("\nLỗi lấy thông tin đăng kí");
                return Content("error");
            }
            //return View("IndexKAP");
        }
    }
}