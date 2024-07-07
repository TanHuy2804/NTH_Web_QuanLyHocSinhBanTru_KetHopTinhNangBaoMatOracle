using NTH_DBHSBT.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NTH_DBHSBT.Controllers
{
    public class DiemDanhController : Controller
    {
        //private string connectionString = Connectring.ConnectionString;
        private string GetConnectionString()
        {
            return Session["Connecstring"] as string;
        }
        private List<DiemDanh> LayDanhSach_DiemDanh()
        {
            List<DiemDanh> ds_cn = new List<DiemDanh>();

            using (OracleConnection connection = new OracleConnection(GetConnectionString()))
            {
                try
                {
                    connection.Open();
                    using (OracleCommand command = new OracleCommand("NV001.v1_DiemDanh_SelectAll", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("C_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DateTime ngayDiemDanh = (DateTime)reader["NgayDiemDanh"];
                                string ngayDiemDanhFormatted = ngayDiemDanh.ToString("dd/MM/yyyy");
                                DiemDanh v1_cn = new DiemDanh
                                {
                                    //NgayDiemDanh = Convert.ToDateTime(reader["NgayDiemDanh"]),
                                    NgayDiemDanh = ngayDiemDanhFormatted,
                                    MaHocSinh = reader["MaHS"].ToString(),
                                    TenHocSinh = reader["TenHS"].ToString(),
                                    TrangThaiDiemDanh = reader["TrangThaiDiemDanh"].ToString(),
                                    GhiChu = reader["GhiChu"].ToString(),
                                    MaNhanVien = reader["MaNV"].ToString(),
                                    MaLop = reader["MaLop"].ToString()
                                };
                                ds_cn.Add(v1_cn);
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

            return ds_cn;
        }

        // GET: v1_DiemDanh
        public ActionResult DiemDanh()
        {
            List<DiemDanh> v1_DiemDanhList = LayDanhSach_DiemDanh();
            return View(v1_DiemDanhList);
        }
    }
}