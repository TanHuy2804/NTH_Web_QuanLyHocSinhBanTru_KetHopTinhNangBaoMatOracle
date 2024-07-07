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
    public class DangNhapController : Controller
    {
        //public string connectionString;

        public ActionResult Connect()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Connect(string txtUser, string txtPassword)
        {
            string status = CheckAccountStatus(txtUser);
            if ("LOCKED".Equals(status) || "LOCKED(TIMED)".Equals(status))
            {
                ViewBag.AccountLocked = true;
                return View("Connect");
            }

            string connectionString = $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=13.82.232.25)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=orcl.aso4bjpgdrmudoy1rf35m0vlgd.bx.internal.cloudapp.net)));User Id={txtUser};Password={txtPassword};"; // Connection string
            //Connectring.ConnectionString = connectionString;
            //user.User = txtUser;
            //Debug.Write("\nUser: " + user.User.ToString());

            //Connectring.ConnectionString = connectionString;
            Session["Connecstring"] = connectionString;

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    Session["Username"] = txtUser;
                    return RedirectToAction("Index", "TrangChu");
                }
                catch (OracleException ex)
                {
                    Debug.Write("OracleException: " + ex.Message);
                    ViewBag.ErrorMessage = "Lỗi kết nối đến cơ sở dữ liệu";
                    return View("Connect");
                }
                catch (Exception ex)
                {
                    Debug.Write("Exception: " + ex.Message);
                    ViewBag.ErrorMessage = "Lỗi kết nối đến cơ sở dữ liệu";
                    return View("Connect");
                }
            }
        }

        public string CheckAccountStatus(string username)
        {
            string connectionString = $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=13.82.232.25)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=orcl.aso4bjpgdrmudoy1rf35m0vlgd.bx.internal.cloudapp.net)));User Id=XacMinhNguoiDung;Password=XacMinhNguoiDung;";

            string status = null;
            try
            {
                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    connection.Open();

                    OracleCommand cmd = new OracleCommand("sys.kiem_tra_trang_thai", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Thêm tham số đầu vào
                    cmd.Parameters.Add("p_ma_nv", OracleDbType.Varchar2).Value = username;

                    // Thêm tham số đầu ra
                    OracleParameter statusParam = new OracleParameter("p_trang_thai", OracleDbType.Varchar2, 100); // Đặt kích thước cho tham số đầu ra
                    statusParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(statusParam);

                    cmd.ExecuteNonQuery();

                    // Lấy giá trị từ tham số đầu ra
                    status = statusParam.Value.ToString();
                }
            }
            catch (OracleException ex)
            {
                // Xử lý ngoại lệ tùy theo yêu cầu của bạn
                Debug.WriteLine("OracleException: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ khác tùy theo yêu cầu của bạn
                Debug.WriteLine("Exception: " + ex.Message);
            }

            return status;
        }

        [HttpGet]
        public ActionResult CheckAndLogout()
        {
            //int sessionExists = CheckUserSession(user.User);

            string username = Session["Username"] as string;
            int sessionExists = CheckUserSession(username);

            if (sessionExists == 0) // Kiểm tra nếu không có phiên làm việc
            {
                Session.Clear(); // Xóa session hiện tại
                // Trả về kết quả JSON với isValid được set thành false
                return Json(new { isValid = 0 }, JsonRequestBehavior.AllowGet);
            }
            // Trả về kết quả JSON với isValid được set thành true
            return Json(new { isValid = 1 }, JsonRequestBehavior.AllowGet);
        }

        // Hàm gọi thủ tục DemSessionsUser để kiểm tra phiên
        public int CheckUserSession(string username)
        {
            int sessionExists = 0;
            string connect = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=13.82.232.25)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=orcl.aso4bjpgdrmudoy1rf35m0vlgd.bx.internal.cloudapp.net)));User Id=XacMinhNguoiDung;Password=XacMinhNguoiDung;";

            using (OracleConnection conn = new OracleConnection(connect))
            {
                try
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("SYS.DemSessionsUsee_Web", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_username", OracleDbType.Varchar2).Value = username;
                        OracleParameter sessionExistsParam = new OracleParameter("p_session_exists", OracleDbType.Int32)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(sessionExistsParam);

                        cmd.ExecuteNonQuery();

                        // Giá trị trả về từ Oracle là INT, không cần chuyển đổi kiểu dữ liệu
                        // Lấy giá trị trả về từ Oracle là INT
                        if (sessionExistsParam.Value != DBNull.Value)
                        {
                            OracleDecimal oracleDecimal = (OracleDecimal)sessionExistsParam.Value;
                            sessionExists = oracleDecimal.ToInt32();
                        }
                        else
                        {
                            Debug.WriteLine("Trống " + sessionExists);
                        }
                        Debug.WriteLine("User session exists: " + sessionExists);
                    }
                }
                catch (OracleException ex)
                {
                    Debug.WriteLine("Oracle Exception: " + ex.Message);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception: " + ex.Message);
                }
            }
            return sessionExists;
        }

        public ActionResult LayTen()
        {
            //string tenHocSinh = LayTenHocSinhTheoMaHS(user.User);
            string username = Session["Username"] as string;
            string tenHocSinh = LayTenHocSinhTheoMaHS(username);
            return Json(new { tenHocSinh = tenHocSinh }, JsonRequestBehavior.AllowGet);
        }
        public string LayTenHocSinhTheoMaHS(string maHS)
        {
            string tenHocSinh = string.Empty;
            string connection = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=13.82.232.25)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=orcl.aso4bjpgdrmudoy1rf35m0vlgd.bx.internal.cloudapp.net)));User Id=XacMinhNguoiDung;Password=XacMinhNguoiDung;";

            using (OracleConnection conn = new OracleConnection(connection))
            {
                try
                {
                    conn.Open();
                    using (OracleCommand command = new OracleCommand("NV001.TenHS_Where_MaHS", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("maHS", OracleDbType.NVarchar2).Value = maHS;
                        command.Parameters.Add("C_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                tenHocSinh = reader["TenHS"].ToString();
                            }
                        }
                    }
                }
                catch (OracleException ex)
                {
                    Debug.WriteLine("Oracle Exception: " + ex.Message);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception: " + ex.Message);
                }
            }

            return tenHocSinh;
        }
    }
}