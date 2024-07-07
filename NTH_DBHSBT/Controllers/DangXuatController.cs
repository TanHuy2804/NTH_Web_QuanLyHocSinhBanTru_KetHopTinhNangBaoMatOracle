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
    public class DangXuatController : Controller
    {
        // GET: DangXuat
        //string connectionString = Connectring.ConnectionString;
        private string GetConnectionString()
        {
            return Session["Connecstring"] as string;
        }
        public ActionResult DangXuat()
        {
            string username = Session["Username"] as string;

            string connectionS = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=13.82.232.25)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=orcl.aso4bjpgdrmudoy1rf35m0vlgd.bx.internal.cloudapp.net)));User Id=XacMinhNguoiDung;Password=XacMinhNguoiDung;";

            // Tạo kết nối đến cơ sở dữ liệu Oracle
            using (OracleConnection connection = new OracleConnection(connectionS))
            {
                try
                {
                    connection.Open();

                    // Tạo một đối tượng OracleCommand để gọi thủ tục đăng xuất
                    using (OracleCommand command = new OracleCommand("SYS.DangXuat", connection))
                    {
                        // Xác định kiểu của command là StoredProcedure
                        command.CommandType = CommandType.StoredProcedure;

                        // Thêm tham số cho thủ tục
                        command.Parameters.Add("p_ma_nhan_vien", OracleDbType.Varchar2).Value = username;

                        // Thực thi thủ tục
                        command.ExecuteNonQuery();
                    }
                    // Xóa thông tin đăng nhập
                    //user.User = null;

                    // Đóng kết nối đến cơ sở dữ liệu nếu có
                    if (!string.IsNullOrEmpty(GetConnectionString()))
                    {
                        using (OracleConnection connection1 = new OracleConnection(GetConnectionString()))
                        {
                            connection1.Close();
                        }
                    }
                    Debug.Write("\nUser name: " + username);
                    return RedirectToAction("Connect", "DangNhap");
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi khác
                    Console.WriteLine("Lỗi: " + ex.Message);
                    // Bổ sung thông báo lỗi cho người dùng
                    ViewBag.ErrorMessage = "Đã xảy ra lỗi không xác định. Vui lòng thử lại sau.";

                    // Điều hướng người dùng đến trang đăng nhập hoặc trang chính của ứng dụng
                    return RedirectToAction("Connect", "DangNhap");
                }
            }
        }
    }
}