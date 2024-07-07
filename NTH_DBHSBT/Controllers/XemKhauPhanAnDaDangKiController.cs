using NTH_DBHSBT.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NTH_DBHSBT.Controllers
{
    public class XemKhauPhanAnDaDangKiController : Controller
    {
        //string connectionString = Connectring.ConnectionString;
        private string GetConnectionString()
        {
            return Session["Connecstring"] as string;
        }
        string maHS, ngayDK, thuTrongTuan, maComBo, qrCode;
        float gia;

        List<KPADaDangKi> dataList = new List<KPADaDangKi>();
        public ActionResult IndexKPA_DaDangKi()
        {
            return View();
        }

        public ActionResult KPADaDangKi()
        {
            List<KPADaDangKi> dataList = GetDataFromDatabase();
            return View("KPADaDangKi", dataList);
        }

        private List<KPADaDangKi> GetDataFromDatabase()
        {
            using (OracleConnection connection = new OracleConnection(GetConnectionString()))
            {

                OracleCommand command = new OracleCommand("NV001.GetKhauPhanDaDangKi", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("KPA", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                try
                {
                    connection.Open();
                    OracleDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        // Đọc dữ liệu từ kết quả truy vấn và thêm vào danh sách
                        KPADaDangKi data = new KPADaDangKi();
                        data.MaHs = reader["MAHS"].ToString();
                        data.maComBo = reader["MACOMBOMON"].ToString();
                        data.ThuTrongTuan = reader["THUTRONGTUAN"].ToString();
                        data.QRCode = reader["QRCODE"].ToString();
                        data.Gia = reader.GetFloat(reader.GetOrdinal("GIA"));

                        data.NgayDangKi = reader["NGAYDANGKYKP"].ToString();

                        dataList.Add(data);
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex.Message);
                }
            }

            return dataList;
        }

        public ActionResult CancelRegistration(string MaHs, string p_NgayDangKyKP, int p_ThuTrongTuan)
        {
            int kq = huyDangKyKPA(MaHs, p_NgayDangKyKP, p_ThuTrongTuan);
            if (kq == 1)
            {
                Debug.WriteLine("\nMÃ HỌC SINH: " + MaHs);
                Debug.WriteLine("\nNgày đăng kí: " + p_NgayDangKyKP);
                Debug.WriteLine("\nThứ trong tuần: " + p_ThuTrongTuan);

                return Content("success");
            }
            else
            {
                return Content("error");
            }
            //return RedirectToAction("KPADaDangKi"); // Chuyển hướng đến view khác nếu cần
        }
        public int huyDangKyKPA(string p_MaHS, string p_NgayDangKyKP, int p_ThuTrongTuan)
        {
            using (OracleConnection connection = new OracleConnection(GetConnectionString()))
            {
                try
                {
                    OracleCommand command = new OracleCommand("NV001.XoaDangKy_MotKhauPhanAn", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    // Thêm các tham số vào thủ tục
                    command.Parameters.Add("p_MaHS", OracleDbType.Varchar2).Value = p_MaHS;
                    command.Parameters.Add("p_NgayDangKyKP", OracleDbType.Varchar2).Value = p_NgayDangKyKP;
                    command.Parameters.Add("p_ThuTrongTuan", OracleDbType.Int32).Value = p_ThuTrongTuan;
                    command.Parameters.Add("p_MaNV", OracleDbType.Varchar2).Value = p_MaHS;
                    connection.Open();
                    command.ExecuteNonQuery();

                    Debug.WriteLine("\nHủy đăng kí thành công.");
                    return 1; // thành công
                }
                catch (OracleException ex)
                {
                    Debug.WriteLine("\nLỗi hủy đăng kí: " + ex.Message);
                    return -1; // lỗi
                }
            }
        }

    }
}