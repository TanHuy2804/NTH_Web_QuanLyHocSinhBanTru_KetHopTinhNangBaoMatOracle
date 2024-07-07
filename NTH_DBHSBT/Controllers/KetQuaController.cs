using NTH_DBHSBT.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NTH_DBHSBT.Controllers
{
    public class KetQuaController : Controller
    {
        //private string connectionString = Connectring.ConnectionString;
        private string GetConnectionString()
        {
            return Session["Connecstring"] as string;
        }
        private List<KetQua> LayDanhSachKetQua()
        {
            List<KetQua> danhSachKetQua = new List<KetQua>();

            using (OracleConnection connection = new OracleConnection(GetConnectionString()))
            {
                try
                {
                    connection.Open();
                    using (OracleCommand command = new OracleCommand("NV001.KetQua_SelectAll", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("C_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DateTime thoiGianLucNop = (DateTime)reader["ThoiGianLucNop"];
                                string thoiGianLucNopFormatted = thoiGianLucNop.ToString("dd/MM/yyyy");
                                KetQua ketQua = new KetQua
                                {
                                    MaHocSinh = reader["MaHocSinh"].ToString(),
                                    MaDeThi = reader["MaDeThi"].ToString(),
                                    SoCauDung = reader["SoCauDung"].ToString(),
                                    DiemSo = reader["DiemSo"].ToString(),
                                    ThoiGianLucNop = thoiGianLucNopFormatted
                                };
                                danhSachKetQua.Add(ketQua);
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

            return danhSachKetQua;
        }

        // GET: KetQua
        public ActionResult KetQua()
        {
            List<KetQua> ketQuaList = LayDanhSachKetQua();
            return View(ketQuaList);
        }
    }
}